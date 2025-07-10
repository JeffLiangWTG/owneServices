using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class JobDeclarationValidation : AutoESJobDeclarationValidation
	{
		public JobDeclarationValidation(JobDeclaration parent)
			: base(parent)
		{
		}

		protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

		protected override void AddMessageErrorIfDeclarantTypeNotEntered()
		{
			if (Parent.CustomsEntryInstructions.Count == 0 || Parent.HasAnyDiffT2CAndT2lEntry)
			{
				base.AddMessageErrorIfDeclarantTypeNotEntered();
			}
		}

		protected override void ValidatePowerOfAttorney()
		{
		}

		protected override void CheckJE_CustomsProfile()
		{
			base.CheckJE_CustomsProfile();

			CertificateHelper.CheckCustomsProfile(Parent.JE_CustomsProfileInfo, Parent.JE_CustomsProfile, Parent.CusAgent);
			if (Parent.HasAnyEXSEntry && !Parent.JE_GS_NKCusAgent.IsEmpty)
			{
				var broker = Parent.Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, Parent.JE_GS_NKCusAgent));
				var brokerWrapper = GlbStaffWrapper.Get(broker);
				var result = brokerWrapper?.ESBPasswordCollection.Cast<GlbExternalPassword>()
							.Any(x => x.GP_Name.EqualsIgnoringCase(Parent.JE_CustomsProfile) && x.GP_MailBoxID.IsEmpty) ?? false;
				if (result)
				{
					Parent.JE_CustomsProfileInfo.AddWarning(Res.GetString("25C21A1B-55E8-44DB-9753-E706662812CB", "The selected certificate has not NIF Certificate value so declaration will be rejected."));
				}
			}
		}

		protected override void CheckJE_PaymentMethodLogicForEU()
		{
		}
	}
}
