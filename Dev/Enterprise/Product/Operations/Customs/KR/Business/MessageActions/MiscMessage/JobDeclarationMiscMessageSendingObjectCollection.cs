using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationMiscMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<JobDeclarationMiscMessageSendingObject>
	{
		public JobDeclarationMiscMessageSendingObjectCollection(JobDeclaration declaration, string messageType, MessageFunctionCode functionCode)
			: base(declaration.Factory)
		{
			this.declaration = declaration;
			PopulateElements(messageType, functionCode);
		}
		readonly JobDeclaration declaration;

		void PopulateElements(string messageType, MessageFunctionCode functionCode)
		{
			RemoveAll();

			var entryLineFilter = GetEntryLineFilter(messageType);
			foreach (CusEntryHeader entry in declaration.ActiveEntryHeaders)
			{
				Add(new JobDeclarationMiscMessageSendingObject(entry, messageType, functionCode, entryLineFilter));
			}
		}

		Func<CusEntryLine, bool> GetEntryLineFilter(string messageType)
		{
			var result = (CusEntryLine entryLine) => true;
			switch (messageType)
			{
				case ElectronicDocumentTypeList.Codes._5SC:
				case ElectronicDocumentTypeList.Codes._105:
				case ElectronicDocumentTypeList.Codes._DHR:
				case ElectronicDocumentTypeList.Codes._DHS:
					result = (CusEntryLine entryLine) => entryLine.CL_FTASequenceNumber > 0;
					break;
				case ElectronicDocumentTypeList.Codes._5FN:
					result = (CusEntryLine entryLine) => entryLine.RandomLine?.IsSubjectTo5FN ?? false;
					break;

				default:
					break;
			}
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotImplementedException();
		protected override bool AllowNewCore => false;
	}
}
