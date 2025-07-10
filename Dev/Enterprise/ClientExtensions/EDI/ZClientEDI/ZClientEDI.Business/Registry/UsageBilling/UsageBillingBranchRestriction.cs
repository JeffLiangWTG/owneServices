using System.Linq;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class UsageBillingBranchRestriction : AutoUsageBillingBranchRestriction
	{
		public UsageBillingBranchRestriction()
		{
		}

		public UsageBillingBranchRestriction(BusinessObjectFactory factory)
			: base(factory) { }

		public UsageBillingBranchRestriction(FallbackLevel fallbackLevel)
			: base(fallbackLevel) { }

		public UsageBillingBranchRestriction(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new UsageBillingBranchRestriction(fallbackLevel, factory);
			return clone;
		}

		public static UsageBillingBranchRestriction GetDefaultValue()
		{
			return new UsageBillingBranchRestriction();
		}

		[List("Lookups.ProductList")]
		public override ZString ProductCode { get => base.ProductCode; set => base.ProductCode = value; }

		[RelatedBusinessObject("InvoicingGlbBranch")]
		[List("Lookups.InvoicingBranches")]
		public override ZGuid InvoicingBranch { get => base.InvoicingBranch; set => base.InvoicingBranch = value; }

		public virtual GlbBranch InvoicingGlbBranch
		{
			get { return (GlbBranch)CurrentFactory.Load(typeof(GlbBranch), InvoicingBranch); }
		}

		#region Validations

		public override void ValidateProductCode()
		{
			base.ValidateProductCode();
			MandatoryValidation.CheckEntered(ProductCodeInfo);
			ListValidation.ErrorIfInvalidCode(ProductCodeInfo);
		}

		public override void ValidateInvoicingBranch()
		{
			base.ValidateInvoicingBranch();
			MandatoryValidation.CheckEntered(InvoicingBranchInfo);

			if (!ProductCode.IsEmpty && !InvoicingBranch.IsEmpty)
			{
				if (ParentCollections.Any(x => x.OfType<UsageBillingBranchRestriction>()
					.Any(y => y.PK != PK && y.ProductCode == ProductCode && y.InvoicingBranch == InvoicingBranch)))
				{
					InvoicingBranchInfo.AddError("The Invoicing Branch must be unique.");
				}
			}
		}

		#endregion

		#region Lookups

		public CodeLookups Lookups
		{
			get => CurrentFactory.GetCachedValue("UsageBillingBranchRestriction.Lookups", () => new CodeLookups(this));
		}

		public class CodeLookups
		{
			public CodeLookups(UsageBillingBranchRestriction parent)
			{
				Parent = parent;
			}

			readonly UsageBillingBranchRestriction Parent;

			public ReadOnlyCodeDescriptionPairList ProductList { get; } = new ProductTypes(false);

			public GlbBranchCollection InvoicingBranches
			{
				get => new GlbBranchCollection(Parent.CurrentFactory);
			}
		}

		#endregion
	}
}
