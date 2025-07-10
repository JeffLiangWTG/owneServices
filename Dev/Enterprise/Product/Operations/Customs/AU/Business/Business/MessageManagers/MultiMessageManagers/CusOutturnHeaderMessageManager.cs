using System.Collections.Generic;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnHeaderMessageManager : MultiMessageManager, Customs.Business.IMessageManager
	{
		public CusOutturnHeaderMessageManager(CusOutturnHeader header)
			: base()
		{
			this.header = header;
			this.SendOnMenuItem = false;
		}

		public override Customs.Business.IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return header; }
		}

		protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers()
		{
			header.MarkAsNeedingValidation();
			header.Outturns.MarkAsNeedingValidation();
			header.RunPreSaveValidation();
			List<Customs.Business.SingleMessageManager> result = new List<Customs.Business.SingleMessageManager>();
			result.Add(new CusUnderbondSEAOUTManager(header));
			return result.ToArray();
		}

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return !SendOnMenuItem; }
		}

		public bool SendOnMenuItem;

		Customs.Business.IDeferredAmendmentSavingOptions Customs.Business.IMessageManager.GetDeferredAmendmentSavingOptions()
		{
			return new DeferredCusOutturnHeaderSavingOptions(header);
		}

		public bool ShouldSendOriginal
		{
			get { return DeclarableManagers.Length > 0; }
		}

		public bool HasSplitMessageOriginalRejectedLog
		{
			get { return header.HasSplitMessageOriginalRejectedLog; }
		}

		public bool HasSplitMessageFailedLog
		{
			get { return header.HasSplitMessageFailedLog; }
		}

		public bool HasNonExistantLineAtCustomsLog
		{
			get { return header.HasNonExistantLineAtCustomsLog; }
		}

		readonly CusOutturnHeader header;
	}
}
