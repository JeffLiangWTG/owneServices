using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.DIF.Business;
using Enterprise.Customs.CA.DIF.Business.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.MasterFiles.Business.DIS;
using NUnit.Framework;

namespace Enterprise.Customs.CA.DIF.GUI.Testing
{
	[TestedType(typeof(MessageSendingForm))]
	sealed class MessageSendingFormTest : MessageSendingFormBaseTest<MessageSendingAction, DIFDocument>
	{
		protected override Form GetFormToBashCore() => new MessageSendingForm(new MessageSendingActionCollection(HostWrapper as DIFHostWrapper));

		protected override MessageSendingFormBase<MessageSendingAction, DIFDocument> GetForm(MessageSendingActionCollectionBase<MessageSendingAction, DIFDocument> collection) => new MessageSendingForm(collection as MessageSendingActionCollection);

		protected override MessageSendingActionCollectionBase<MessageSendingAction, DIFDocument> GetCollection() => new MessageSendingActionCollection(HostWrapper as DIFHostWrapper);

		DIFHostWrapper hostWrapper;
		protected override DISHostWrapperBase<DIFDocument> HostWrapper => hostWrapper ?? (hostWrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration));

		BusinessObject jobDeclaration;
		protected override BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
