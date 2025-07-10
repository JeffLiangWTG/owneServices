using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class PackageValidation : CusDecHouseContainerPackValidation
	{
		public PackageValidation(Package parent)
			: base(parent)
		{
		}

		public new Package Parent
		{
			get { return (Package)base.Parent; }
		}

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAtLeastOneInvoiceOrInvoiceLineLinked();
		}

		#endregion

		#region CheckCW_PackQty

		protected override void CheckCW_PackQty()
		{
			base.CheckCW_PackQty();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CW_PackQtyInfo, Res.GetString("995517d1-bd87-4fba-9417-9a85db1f788f", "Quantity of Packages"));
			CheckEnterAtLeastOnePackWithQuantityAndPackType();
		}

		#endregion

		#region CheckCW_CW_Parent

		protected override void CheckCW_CW_Parent()
		{
			base.CheckCW_CW_Parent();
			var ancestors = Parent.Ancestors;
			var count = ancestors.Take(MaxLevelsAllowed + 1).Count();

			if (count >= MaxLevelsAllowed)
			{
				Package.CW_CW_ParentInfo.AddMessageError(MaxPackingHeirarchyError);
			}

			ListValidation.ErrorIfInvalidPK(Parent.CW_CW_ParentInfo, Parent.Lookups.IrrelevantPackages);
		}

		public static string MaxPackingHeirarchyError => Res.GetString("c5258d66-9e53-40cf-b790-8753ac9af02a", "Packing hierarchy is above the limit of 3 levels");
		const int MaxLevelsAllowed = 3;

		#endregion

		#region CheckCW_PackType

		protected override void CheckCW_PackType()
		{
			base.CheckCW_PackType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CW_PackTypeInfo, Parent.PackTypeList);
			CheckEnterAtLeastOnePackWithQuantityAndPackType();
		}

		#endregion

		protected override void CheckCW_MarksAndNos()
		{
			base.CheckCW_MarksAndNos();
			if (Parent.CW_MarksAndNos.HasCharactersNotSupportedByCAMessaging())
			{
				Parent.CW_MarksAndNosInfo.AddWarning(Res.GetString("1e5b2bfc-e685-4423-9e6b-248dea4be183", "The Marks has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs."));
			}

			var declaration = (JobDeclaration)Parent.Declaration;
			if (declaration != null && declaration.IsIID && Parent.CW_MarksAndNos.Length > 35)
			{
				Parent.CW_MarksAndNosInfo.AddMessageError(Res.GetString("3206b4fb-3f68-4d48-924c-c383083ac9c0",
					"Marks cannot be longer than 35 characters."));
			}
		}

		#region CheckEnterAtLeastOnePackWithQuantityAndPackType

		void CheckEnterAtLeastOnePackWithQuantityAndPackType()
		{
			Parent.RemoveRowMessageError(EnterAtLeastOnePackWithQuantityAndPackType);
			if (Parent.CW_PackQty.IsEmpty || Parent.CW_PackType.IsEmpty)
			{
				var declaration = (JobDeclaration)Parent.Declaration;
				if (declaration != null && declaration.DeclarationValidator.IsValidationRequired(ValidateForMessageType.ACROSS))
				{
					if (!declaration.Packages.Cast<Package>().Any(p => !(p.CW_PackQty.IsEmpty || p.CW_PackType.IsEmpty)))
					{
						Parent.AddRowMessageError(EnterAtLeastOnePackWithQuantityAndPackType);
					}
				}
			}
		}

		public static string EnterAtLeastOnePackWithQuantityAndPackType
		{
			get { return Res.GetString("45b5a2d7-bb0a-46f3-9c94-54f38be86efc", "Please enter at least one pack line with quantity and pack type."); }
		}

		#endregion

		#region CheckAtLeastOneInvoiceLineLinked

		internal void ValidateAtLeastOneInvoiceOrInvoiceLineLinked()
		{
			Parent.RemoveRowMessageError(LinkAtLeastOneInvoiceOrInvoiceLine);
			var declaration = Parent.Declaration as JobDeclaration;

			if (Parent.IsLowestPackage && declaration != null)
			{
				if ((declaration.SupportsChcPivotBetweenInvoiceLineAndPacking
				&& declaration.InvoiceLines.Any()
				&& !declaration.InvoiceLines.Cast<JobComInvoiceLine>().Any(invoiceLine => invoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>().Any(pivot => pivot.CHC_CW == Parent.PK)))
				&&
				(declaration.SupportsChzPivotBetweenInvoiceHeaderAndPacking
				&& declaration.Invoices.Any()
				&& !declaration.Invoices.Cast<JobComInvoiceHeader>().Any(invoice => invoice.PackagesPivot.Cast<InvoiceHeaderPackagePivot>().Any(pivot => pivot.CHZ_CW == Parent.PK)))
				)
				{
					Parent.AddRowMessageError(LinkAtLeastOneInvoiceOrInvoiceLine);
				}
			}
		}

		public static string LinkAtLeastOneInvoiceOrInvoiceLine
		{
			get
			{
				return Res.GetString("50213454-129c-41d8-b9c6-047252abeb64", "At least one invoice or invoice line should be linked.");
			}
		}
		#endregion
	}
}
