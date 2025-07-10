using System;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public abstract class NCTSMessagePrettier<TMessage> : UCCMessagePrettier<TMessage>
		where TMessage : class
	{
		protected NCTSMessagePrettier(NCTSMessageDataObject<TMessage> messageDataObject) : base(messageDataObject)
		{
		}

		public new NCTSMessageDataObject<TMessage> MessageDataObject => (NCTSMessageDataObject<TMessage>)base.MessageDataObject;

		public ZString GetErrorCodeNumber(ZString errorCode)
		{
			return errorCode.KeepNumericCharacters();
		}

		public ZString GetMessageTypeDescription(ZString messageType)
		{
			if (messageType.IsEmpty)
			{
				return ZString.Empty;
			}	
			return GetTP5ResponseMessageSubTypeList()[messageType.KeepNumericCharacters()]?.Description ?? ZString.Empty;
		}

		public ZString GetRemark(ZString errorCode)
		{
			var remark = ZString.Empty;
			var errorCodeNumber = GetErrorCodeNumber(errorCode);
			if (!string.IsNullOrEmpty(errorCodeNumber))
			{
				var cusCodeList = GetCL180CodeList(errorCodeNumber);
				var attribute = cusCodeList?.Attributes?.Cast<ZZRefCusCodeListAttributeCombined>().FirstOrDefault(x => string.Compare(x.ZZE_ZXE_NKName, UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.Remark, StringComparison.OrdinalIgnoreCase) == 0);
				return attribute?.ZZE_Value ?? ZString.Empty;
			}

			return remark;
		}

		public ZString GetErrorCodeDescription(ZString errorCode)
		{
			var errorCodeNumber = GetErrorCodeNumber(errorCode);
			return string.IsNullOrEmpty(errorCodeNumber) ? ZString.Empty : GetCL180CodeList(errorCodeNumber)?.ZZD_Description ?? ZString.Empty;
		}

		ZZRefCusCodeListCombined GetCL180CodeList(ZString errorCode)
		{
			return Factory.GetCachedValue(string.Format(CultureInfo.InvariantCulture, "GetCL180CodeList_{0}", errorCode), () =>
			{
				return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(MessageDataObject.Factory, errorCode, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL180, ZDateTime.Today);
			});
		}

		public ZString GetNCTS5InvalidGuaranteeReasonDescription(ZString reasonCode)
		{
			var list = GetNCTS5InvalidGuaranteeReasonList();
			if(!reasonCode.IsEmpty && list.ContainsCode(reasonCode))
			{
				return list.GetDescriptionFromCode(reasonCode);
			}
			return ZString.Empty;
		}

		TP5ResponseMessageSubTypeList GetTP5ResponseMessageSubTypeList() => tP5ResponseMessageSubTypeList ?? (tP5ResponseMessageSubTypeList = new TP5ResponseMessageSubTypeList());
		TP5ResponseMessageSubTypeList tP5ResponseMessageSubTypeList;

		NCTS5InvalidGuaranteeReason GetNCTS5InvalidGuaranteeReasonList() => nCTS5InvalidGuaranteeReasonList ??= new NCTS5InvalidGuaranteeReason();
		NCTS5InvalidGuaranteeReason nCTS5InvalidGuaranteeReasonList;
	}
}
