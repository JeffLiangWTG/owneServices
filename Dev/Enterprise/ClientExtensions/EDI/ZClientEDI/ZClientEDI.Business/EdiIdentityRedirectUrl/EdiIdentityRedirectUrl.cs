using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IdentityRedirectUrl.Business
{
	public class EdiIdentityRedirectUrl : AutoEdiIdentityRedirectUrl
	{
		public EdiIdentityRedirectUrl(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[MaxLength(200)]
		public override ZString IAR_RedirectUrl
		{
			get => base.IAR_RedirectUrl;
			set => base.IAR_RedirectUrl = value;
		}

		[List("Lookups.RedirectTypes")]
		public override ZString IAR_RedirectType
		{
			get => base.IAR_RedirectType;
			set => base.IAR_RedirectType = value;
		}

		public EdiIdentityApplication EdiIdentityApplication
		{
			get { return Factory.Load<EdiIdentityApplication>(IAR_IDA); }
		}

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				EdiIdentityApplication.Logs.AddNew(AutoEvents.AddedARecordToTheSystem,
					$"Added : " + LogInfoMessge);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}

			EdiIdentityApplication.IDA_RedirectUrlStatus = EdiIdentityApplicationRedirectUrlStatus.Codes.Nudged;
			base.OnSaving();
		}

		public override void Delete()
		{
			if (IsInDatabase)
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				EdiIdentityApplication.Logs.AddNew(AutoEvents.DeletedARecordInTheSystem,
					$"Deleted : " + LogInfoMessge);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

				EdiIdentityApplication.IDA_RedirectUrlStatus = EdiIdentityApplicationRedirectUrlStatus.Codes.Nudged;
			}

			base.Delete();
		}

		string LogInfoMessge => $"ApplicationName={IAR_ApplicationName}|RedirectUrl={IAR_RedirectUrl}|RedirectType={IAR_RedirectType}";
	}
}
