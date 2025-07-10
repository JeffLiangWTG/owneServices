using CargoWise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class EntryHeaderTypeListProvider : Integration.Customs.ICACusEntryHeaderTypeListProvider
	{
		public ICodeDescriptionPairList GetEntryTypes()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Descriptions.B3CUSDEC);
			result.AddPair(MessageTypeList.Codes.CommercialAccountingDeclaration, MessageTypeList.Descriptions.CommercialAccountingDeclaration);
			result.AddPair(MessageTypeList.Codes.G7Export, MessageTypeList.Descriptions.G7Export);
			result.AddPair(MessageTypeList.Codes.EDIRelease, MessageTypeList.Descriptions.EDIRelease);

			return result;
		}
	}
}
