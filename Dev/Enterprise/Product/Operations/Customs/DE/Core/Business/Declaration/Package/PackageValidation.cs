using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class PackageValidation : EU.Business.Declaration.PackageValidation
	{
		public PackageValidation(Package parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidatePackageIsAssignedToInvoiceLine();
		}

		protected new Package Parent => (Package)base.Parent;

		protected override void CheckCW_PackType()
		{
			base.CheckCW_PackType();
			var parent = Parent;
			if (parent.Declaration?.IsImport ?? false)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.CW_PackTypeInfo);
			}
		}

		protected override void CheckCW_MarksAndNos()
		{
			base.CheckCW_MarksAndNos();

			var parent = Parent;
			if (parent.Declaration is JobDeclaration declaration)
			{
				var marks = parent.CW_MarksAndNos;
				var propertyInfo = parent.CW_MarksAndNosInfo;
				var marksMaxLength = declaration.IsImport ? 70 : declaration.IsTransitionPeriodAES30 ? 42 : 512;

				if (marks.Length > marksMaxLength)
				{
					propertyInfo.AddMessageError(Res.GetString("2A0E84E1-8CE5-4554-AE7A-A73D7426F598", "The maximum length for Marks is {0} characters.", marksMaxLength));
				}
			}
		}

		void ValidatePackageIsAssignedToInvoiceLine()
		{
			var parent = Parent;
			if ((parent.Declaration?.IsImport ?? false) && parent.InvoiceLinePivotCollection.Count == 0)
			{
				parent.AddRowMessageError(Res.GetString("B0736CDA-08E0-4635-80D1-3CC348CCBC51", "Package Line is not assigned to an Invoice Line."));
			}
		}
	}
}
