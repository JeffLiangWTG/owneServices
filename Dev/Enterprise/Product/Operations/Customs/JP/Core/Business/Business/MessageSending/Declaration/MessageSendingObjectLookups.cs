using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Business
{
	public class MessageSendingObjectLookups : ZLookups
	{
		public MessageSendingObjectLookups(BusinessObject parent) : base(parent)
		{
		}

		public new MessageSendingObject Parent => (MessageSendingObject)base.Parent;

		public CodeDescriptionPairList ProcedureCodeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var declaration = Parent.Header.Declaration;
				if (declaration != null)
				{
					if (declaration.IsImport)
					{
						result = Factory.GetCachedValue<JPProcedureCodeList.ImportMessageSendingObjectProcedureCodeList>();
					}
					else if (declaration.IsExport)
					{
						result = Factory.GetCachedValue<JPProcedureCodeList.ExportMessageSendingObjectProcedureCodeList>();
					}
				}

				return result;
			}
		}

		public CodeDescriptionPairList MessageActionList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				var exportControlNumber = Parent.Header.EntryInstruction?.ExportControlNumber;
				if (string.IsNullOrEmpty(exportControlNumber))
				{
					result.AddPair(ActionList.Codes.Nine, ActionList.Descriptions.Nine);
				}
				else
				{
					result.AddPair(ActionList.Codes.Five, ActionList.Descriptions.Five);
					result.AddPair(ActionList.Codes.One, ActionList.Descriptions.One);
				}

				return result;
			}
		}
	}
}
