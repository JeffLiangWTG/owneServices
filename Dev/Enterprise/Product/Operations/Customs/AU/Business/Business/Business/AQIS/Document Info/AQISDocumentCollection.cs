using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISDocumentCollection : AQISCollection<AQISDocument>
	{
		public AQISDocumentCollection(BusinessObjectFactory factory, AUAddInfo addInfo)
			: base(factory, addInfo)
		{
		}

		protected override BusinessObject BusinessObjectToAddToCollection(ZString value1, ZString value2)
		{
			AQISDocument document = new AQISDocument(Factory);
			document.Type = value1;
			document.Number = value2;

			return document;
		}

		public override void ReBuildAndSaveAQISElements()
		{
			ZStringBuilder result = new ZStringBuilder();

			foreach (AQISDocument document in this)
			{
				result.Append(document.Type.ToString() + "/" + document.RawNumber + ",");
			}

			AddInfo.ZA_AQISDocuments_Hidden = new ZString(result.ToString()).TrimEndIncludingWhiteSpace(',');
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AQISDocument(Factory);
		}
	}
}
