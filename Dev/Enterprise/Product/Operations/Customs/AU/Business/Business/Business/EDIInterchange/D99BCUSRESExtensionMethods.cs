using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class D99BCUSRESExtensionMethods
	{
		public static ZString GetRelatedDocumentType(this CUSRESMessage cUSRES)
		{
			foreach (FTXSegment fTX in cUSRES.FTX)
			{
				if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.MessageTypeName.ToString())
				{
					return fTX.TextLiteral.FreeTextValue1;
				}
			}
			return CMRMessage.CMRMessageTypes.ERM;
		}
	}
}
