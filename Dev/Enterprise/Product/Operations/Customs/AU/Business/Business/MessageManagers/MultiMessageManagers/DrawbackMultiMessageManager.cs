using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DrawbackMultiMessageManager : MultiMessageManager
	{
		public DrawbackMultiMessageManager(JobDeclaration declaration, string messageSubType)
			: base()
		{
			this.declaration = declaration;
			this.MessageSubType = messageSubType;
		}
		readonly JobDeclaration declaration;
		public readonly string MessageSubType;

		public override Customs.Business.IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return declaration; }
		}

		public bool CanSendOriginalMessage
		{
			get
			{
				foreach (DrawbackMessageManager singleManager in AllMessageManagers)
				{
					if (singleManager.CanSendOriginal)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool SendMessage(Customs.Business.ISendsMessagesToCustoms sender)
		{
			bool result = false;
			if (MessageSubType == CMRMessage.MessageSubTypes.Original)
			{
				result = SendOriginalMessages(sender).Any();
			}
			else
			{
				throw new ArgumentException("Invalid Drawback Message Manager MessageSubType: " + MessageSubType);
			}
			return result;
		}

		#region Implementation
		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return true; }
		}

		protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers()
		{
			List<Customs.Business.SingleMessageManager> result = new List<Customs.Business.SingleMessageManager>();
			if (declaration != null)
			{
				result.Add(new DrawbackMessageManager(declaration));
			}
			return result.ToArray();
		}
		#endregion
	}
}

//TODO: tests to be completed
