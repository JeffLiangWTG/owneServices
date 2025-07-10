using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessageSending;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CC414BWrapper : ICC414B
	{
		protected CC414BWrapper(DeltaIEJobDeclarationMessageSendingObject messageObject, bool isForOperationalAction = false)
		{
			this.messageObject = Argument.NotNull(messageObject, nameof(messageObject));
			this.entryHeader = Argument.NotNull(messageObject.Header, nameof(entryHeader));
			this.declaration = Argument.NotNull(entryHeader.Declaration, nameof(declaration));
			this.isForOperationalAction = isForOperationalAction;
		}
		readonly protected DeltaIEJobDeclarationMessageSendingObject messageObject;
		readonly protected CusEntryHeader entryHeader;
		readonly protected JobDeclaration declaration;
		readonly bool isForOperationalAction;

		public static CC414BWrapper New(DeltaIEJobDeclarationMessageSendingObject messageObject, bool isForOperationalAction = false) => messageObject == null ? null : new CC414BWrapper(messageObject, isForOperationalAction);

		public IDeclarant Declarant => declarant ?? (declarant = GetDeclarantCore());

		protected IDeclarant GetDeclarantCore() => isForOperationalAction ? DeclarantWrapper.New(true) : DeclarantWrapper.New(declaration);

		IDeclarant declarant;

		public ICollection<ICC414BCciOperation> ImportOperation => importOperation ?? (importOperation = GetNewImportOpertaionForSingleMessage());
		ICollection<ICC414BCciOperation> importOperation;

		public ICollection<ICC414BCciOperation> GetNewImportOpertaionForSingleMessage()
		{
			var opertionCollection = new List<ICC414BCciOperation>();
			opertionCollection.Add(CC414BCciOperationWrapper.New(messageObject));

			return opertionCollection;
		}

		public IRepresentative Representative => representative ?? (representative = RepresentativeWrapper.New(declaration));
		IRepresentative representative;

		public IRequest Request => RequestWrapper.New(messageObject);
	}
}
