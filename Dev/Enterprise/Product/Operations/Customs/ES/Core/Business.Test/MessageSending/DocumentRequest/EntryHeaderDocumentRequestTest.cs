using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public abstract class EntryHeaderDocumentRequestTest<T> : CommonDocumentRequestTest<T, CusEntryHeader>
		where T : EntryHeaderDocumentRequest
	{
		public void TestConstructorDeclaration()
		{
			AssertExceptionThrown<ArgumentNullException>("declaration null", () => GetDocumentRequestClass(Factory.New<CusEntryHeader>(), "A"));
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.ZG_IsTrainingDeclaration = true;
			businessObject = declaration.CustomsEntryHeaders.AddNew();
			businessObject.MovementReferenceNumberSetter(mrnCode, ZDateTime.Today);
			businessObject.Factory.Save();
		}

		protected JobDeclaration declaration;

		protected override EDIMessage GetLastMessage(CusEntryHeader businessObject)
		{
			businessObject.Messages.Reload(true);
			return businessObject.Messages.LastMessage;
		}

		protected override ZBool IsTrain() => true;
	}
}
