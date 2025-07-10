using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class BREDIMessage : EDIMessage, Integration.Customs.BR.IEDIMessage
	{
		public BREDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool IsImportLicense => EM_MessageType == BRJobMessageTypeList.Codes.ImportLicense;

		public bool IsProductMessage => EM_MessageType == MessageTypeList.Codes.CAT && EM_MessageSubType == EDIMessageSubTypeList.Codes.Original;

		public bool IsForeignOperatorMessage => EM_MessageType == MessageTypeList.Codes.OPE && EM_MessageSubType == EDIMessageSubTypeList.Codes.Original;

		public bool IsProductLinkMessage => EM_MessageType == MessageTypeList.Codes.CAT && EM_MessageSubType == EDIMessageSubTypeList.Codes.Link;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.BRCustoms;
		}

		protected override string GetMessageReferenceNumber()
		{
			return Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", ApplicationCodes.BRCustoms).GetNextFormatted(Factory);
		}

		protected override void PopulateMessageNumber()
		{
			if (EM_MessageNum.IsEmpty)
			{
				base.PopulateMessageNumber();
			}
		}

		protected override CodeDescriptionPairList MessageSubTypeList => Factory.GetCachedValue<EDIMessageSubTypeList>();

		protected override bool ShouldReplaceUniqueBatchNumber => false;
	}
}
