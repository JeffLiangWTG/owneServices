using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Integration.Customs.CA;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CACusRuling : ZZRefCusRulingCombined, ICusRuling
	{
		public CACusRuling(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("F1A35864-1EE1-42E1-879E-90CFD651807A", Caption = "Remission Type")]
		public override ZString ZZX_RulingType
		{
			get => base.ZZX_RulingType;
			set
			{
				var oldValue = base.ZZX_RulingType;
				if (oldValue != value)
				{
					base.ZZX_RulingType = value;
					if (ZZX_RulingType != RefCusRulingTypeList.Codes.T)
					{
						Configurations.RemoveAndDeleteAll();
					}
				}
			}
		}

		[ResourceStringData("DFCE8452-8025-4BB8-9A88-D860D6D0CE14", Caption = "Remission Number")]
		public override ZString ZZX_RulingNumber { get => base.ZZX_RulingNumber; set => base.ZZX_RulingNumber = value; }

		protected override ZString HumanReadableShortcutNameCorePrefix => Res.GetString("70063B5B-D822-4F13-8240-D5E02CCC75AD", "OIC - {0}", ZZX_RulingNumber);

		protected override ZString HumanReadableNameCore => Res.GetString("CACusRulingForm|FormCaption", "Remissions");

		public new CACusRulingConfigCollection Configurations => (CACusRulingConfigCollection)base.Configurations;

		ICACusRulingConfigCollection ICusRuling.Configurations => Configurations;

		protected override ZBool ConfigurationsReadOnly => ZZX_RulingType != RefCusRulingTypeList.Codes.T;

		protected override CusRulingConfigCombinedCollection GetConfigurationsCore()
		{
			return new CACusRulingConfigCollection(this);
		}

		#region Validation
		public new CACusRulingValidation Validation => (CACusRulingValidation)base.Validation;

		protected override ZZRefCusRulingCombinedValidation GetNewValidation()
		{
			return new CACusRulingValidation(this);
		}
		#endregion

		public new CACusRulingLookups Lookups => (CACusRulingLookups)base.Lookups;

		protected override ZZRefCusRulingCombinedLookups GetNewLookups()
		{
			return new CACusRulingLookups(this);
		}
	}
}
