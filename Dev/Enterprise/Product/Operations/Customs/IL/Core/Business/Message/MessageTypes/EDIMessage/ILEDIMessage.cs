using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.IL.Business
{
	public class ILEDIMessage : EDIMessage, Integration.Customs.IL.IEDIMessage
	{
		public ILEDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ThreadSafe]
		public new static readonly ILEDIMessageTypeDecider TypeDecider = new ILEDIMessageTypeDecider();

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get => (!MessageDataObject?.CanSetMessageInterpretation ?? true) && !EM_MessageText.IsEmpty && MessageDataObject?.Prettier != null
				? MessageDataObject?.Prettier.GetMessageInterpretation() ?? ZString.Empty
				: base.EM_MessageInterpretation;
		}

		#region MessageDataObject

		public MessageDataObjectBase MessageDataObject => messageDataObject ?? (messageDataObject = GenerateMessageDataObject());
		MessageDataObjectBase messageDataObject;

		protected virtual MessageDataObjectBase GenerateMessageDataObject()
		{
			return null;
		}

		#endregion MessageDataObject

		protected override string GetMessageReferenceNumber() => Env.NumberFountains.ILMessageControlNumber(GlbCompany.CurrentCompany.PK.ToGuid()).GetNextFormatted(Factory);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ILEDIInterchange.ApplicationCodes.ILCustoms;
		}

		protected override CodeDescriptionPairList MessageSubTypeList => Factory.GetCachedValue<ILEDIMessageSubTypeList>();

		protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride() => ZBool.True;
	}
}
