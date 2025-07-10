using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.JP.Common
{
	[SystemDefinedValues]
	[CodeProperty(nameof(GP_CodeProperty)), DescriptionProperty(nameof(GP_Transport))]
	public class GlbExternalPasswordCUS : GlbExternalPasswordWithPasswordType
	{
		public GlbExternalPasswordCUS(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : GlbExternalPassword.Schema
		{
			public const string GP_Transport = "GP_Transport";
			public const int GP_TransportMaxLength = 3;
		}

		public ZString GP_CodeProperty => GP_MailBoxID + GP_UserID;

		[List(nameof(Lookups) + "." + nameof(GlbExternalPasswordCUSLookups.TransportModeList))]
		[MaxLength(Schema.GP_TransportMaxLength)]
		public ZString GP_Transport
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.GP_Transport);
			set
			{
				var oldValue = GP_Transport;
				if (value != oldValue)
				{
					CheckMaximumLength(GP_TransportInfo, value);
					this.SetSystemDefinedValue(Schema.GP_Transport, value);
					Validation.ValidateGP_Transport();

					GP_TransportInfo.RefreshBinding(oldValue);
				}
			}
		}

		public ZPropertyInfo GP_TransportInfo
		{
			get { return GetZPropertyInfo(Schema.GP_Transport); }
		}

		[List(nameof(Lookups) + "." + nameof(GlbExternalPasswordLookups.PasswordTypeList))]
		public override ZString GP_PasswordType { get => base.GP_PasswordType; set => base.GP_PasswordType = value; }

		public override string PasswordTypeCode => JPPasswordType.Codes.CUS;

		public override string PasswordTypeDescription => JPPasswordType.Descriptions.CUS;

		public new GlbExternalPasswordCUSValidation Validation => (GlbExternalPasswordCUSValidation)base.Validation;

		protected override GlbExternalPasswordValidation GetNewValidation() => new GlbExternalPasswordCUSValidation(this);

		public new GlbExternalPasswordCUSLookups Lookups => (GlbExternalPasswordCUSLookups)base.Lookups;

		protected override GlbExternalPasswordLookups GetNewLookups() => new GlbExternalPasswordCUSLookups(this);
	}
}
