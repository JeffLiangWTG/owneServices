using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public abstract class EntryHeaderDocumentRequest : CommonDocumentRequest<CusEntryHeader>
	{
		public EntryHeaderDocumentRequest(CusEntryHeader entryHeader, ZString certName) : base(entryHeader, certName)
		{
			declaration = Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
		}

		protected readonly JobDeclaration declaration;

		protected sealed override ZString GetBGMReference(CusEntryHeader businessObject) => businessObject.CH_BGMReference;

		protected sealed override ZString GetCSVReference(CusEntryHeader businessObject) => businessObject.CSVClearance;

		protected sealed override EDIMessageCollection GetMessages(CusEntryHeader businessObject) => businessObject.Messages;

		protected sealed override ZBool GetIsTrainingDeclaration()
		{
			var registration = ObjectFactory.Get<IProductRegistration>();
			return registration.Key.DatabaseType != DatabaseTypes.Codes.Production
						&& (!registration.IsWiseTechGlobalInternalSystem()
							|| (bool)declaration.ZG_IsTrainingDeclaration);
		}

		protected override GlbStaff GetBroker() => declaration.CusAgent;
	}
}
