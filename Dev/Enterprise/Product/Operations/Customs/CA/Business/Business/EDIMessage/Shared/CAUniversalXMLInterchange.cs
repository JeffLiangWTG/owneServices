using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CAUniversalXMLInterchange : Enterprise.Messaging.Business.EDIInterchange
	{
		#region Constants

		public const string SenderNetworkIDPlaceHolder = "<<SenderNetworkIDPlaceHolder>>";
		public const string RecipientNetworkIDPlaceHolder = "<<RecipientNetworkIDPlaceHolder>>";

		#endregion

		public CAUniversalXMLInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Fields

		ZString senderNetworkID;
		ZString recipientNetworkID;

		#endregion

		#region override

		protected override bool ShouldSendViaEHubCore
		{
			get { return true; }
		}

		protected override ZString GetInterchangeNumber()
		{
			return ZDateTime.Now.ToString("yyyyMMddHHmm", CultureInfo.CurrentCulture) + BasicInterchangeNumber.PadLeft(14, '0');
		}

		public override void OnSaving()
		{
			base.OnSaving();
			var interchangeNumberReplacementString = GetInterchangeNumberReplacementString(BasicInterchangeNumber);
			EI_BodyText = EI_BodyText.Replace(SenderNetworkIDPlaceHolder, senderNetworkID)
			.Replace(RecipientNetworkIDPlaceHolder, recipientNetworkID)
			.Replace(InterchangeNumberPlaceHolder, interchangeNumberReplacementString);
		}

		protected override bool ShouldBatchNumberBeByInterchange
		{
			get { return CACustomsDataRegistry.Instance.ShouldBatchNumberBeByInterchange.Value; }
		}

		#endregion

		#region Implemetation

		ZString BasicInterchangeNumber
		{
			get { return basicInterchangeNumber ?? (basicInterchangeNumber = GetInterchangeNumberFromFountain()); }
		}
		string basicInterchangeNumber;

		ZString GetInterchangeNumberFromFountain()
		{
			var pivotMessage = (this.ContainedMessages != null && this.ContainedMessages.Count > 0) ? this.ContainedMessages[0] : null;
			var branchPK = pivotMessage == null ? GlbBranch.CurrentBranch.PK.ToGuid() : pivotMessage.EM_GB.ToGuid();
			var isTestMode = !Env.Instance.IsProductionSystem;

			using (DisposableEnvironment.ForBranch(branchPK))
			{
				senderNetworkID = BatchProcessor.BatchProcessorUtilities.MailBoxID;
				recipientNetworkID = BatchProcessor.BatchProcessorUtilities.CBSAClientID(isTestMode);
			}
			return Env.NumberFountains.EDIFACTNumberFountain("I", senderNetworkID, recipientNetworkID).GetNextFormatted(Factory).ToUpper(CultureInfo.CurrentCulture);
		}

		#endregion
	}
}
