using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class NctsHeaderSharedDataProvider
	{
		protected readonly NctsHeader nctsHeader;

		public NctsHeaderSharedDataProvider(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		public string CustomsOfficeOfDeparture => nctsHeader.IsPhase5 ? nctsHeader.CommonMovementHeader.DepartureCustomsOfficeCode : nctsHeader.DepartureCustomsOfficeCode;

		public virtual string CustomsOfficeOfDestination => nctsHeader.IsPhase5 ? nctsHeader.CommonMovementHeader.DestinationCustomsOfficeCode : nctsHeader.DestinationCustomsOfficeCode;

		public string MRN => nctsHeader.MovementReferenceNumber;

		public string MessageSender => GetMessageSender();

		public virtual string MessageRecipient { get; }

		public DateTime PreparationDateTime
		{
			get
			{
				var now = ZDateTime.Now.ToUniversalBranchTime();
				return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
			}
		}

		public string MessageIdentification => EDIMessage.SendersReferencePlaceHolder;

		public virtual string MessageType => string.Empty;

		public string CorrelationIdentifier => MessageIdentification;

		string GetMessageSender()
		{
			var result = string.Empty;
			if (!nctsHeader.Branch.GB_OH_OrgProxy.IsEmpty)
			{
				result = nctsHeader.Branch.OrgProxy?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) ??
					GlbBranch.CurrentBranch.OrgProxy?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) ?? ZString.Empty;
			}
			if (string.IsNullOrEmpty(result)
				&& !GlbCompany.CurrentCompany.GC_OH_OrgProxy.IsEmpty)
			{
				result = GlbCompany.CurrentCompany.OrgProxy?.GetRegoCodeOfThisOrg(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori) ?? ZString.Empty;
			}
			return result;
		}
	}
}
