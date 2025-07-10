using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.KR.Business
{
	public class EDIInterchange : Enterprise.Messaging.Business.EDIInterchange, Integration.Customs.KR.IEDIInterchange
	{
		public EDIInterchange(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override void OnSaving()
		{
			base.OnSaving();
			var oldValue = (ZString)EI_StatusInfo.OriginalValue;
			if (!IsErrorOrFailStatus(oldValue))
			{
				if (IsErrorOrFailStatus(EI_Status))
				{
					var linkedObject = ContainedMessages[0]?.EM_LinkedObject as IEDIMessageCollectionProviderWithID;
					if (linkedObject != null)
					{
						linkedObject.MarkAsFailed();
					}
				}
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = ApplicationCodes.KRCustoms;
		}

		static bool IsErrorOrFailStatus(ZString status)
		{
			var result = false;
			if (status == EDIInterchangeStatusList.Codes.Failed || status == EDIInterchangeStatusList.Codes.Error)
			{
				result = true;
			}
			return result;
		}
	}
}
