using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class EComplaintMessageSendingObjectLineLookups : ZLookups
{
	public EComplaintMessageSendingObjectLineLookups(EComplaintMessageSendingObjectLine parent) : base(parent)
	{
	}

	new EComplaintMessageSendingObjectLine Parent => (EComplaintMessageSendingObjectLine)base.Parent;

	public CodeDescriptionPairList LocationList => Factory.GetCachedValue<EComplaintLocationList>();

	public CodeDescriptionPairList FieldNameList
	{
		get
		{
			ZString attributeValue = ZString.Empty;
			switch (Parent.Location)
			{
				case EComplaintLocationList.Codes.Header:
					attributeValue = UniversalReferenceConstants.RefCusCodeList.AttributeValues.Yes;
					break;
				case EComplaintLocationList.Codes.Line:
					attributeValue = UniversalReferenceConstants.RefCusCodeList.AttributeValues.No;
					break;
			}
			return attributeValue.IsEmpty ? new CodeDescriptionPairList()
				: RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory, Core.Constants.CountryCodes.Switzerland,
						UniversalReferenceConstants.RefCusCodeList.EdecTypes.EComplaintFields, ZDateTime.Today, false,
						UniversalReferenceConstants.RefCusCodeList.Attributes.IsHeader, new[] { attributeValue });
		}
	}

	public CodeDescriptionPairList EntryLineList
	{
		get
		{
			if (fEntryLineList == null)
			{
				fEntryLineList = new CodeDescriptionPairList();
				foreach (var entryLine in Parent.EntryHeader.MergedLines.OrderBy(x => x.CL_LineNumber))
				{
					fEntryLineList.AddPair(entryLine.PK, entryLine.CL_LineNumber.ToString(), entryLine.CL_AdValoremTariff + " " + entryLine.CL_Description);
				}
			}
			return fEntryLineList;
		}
	}
	CodeDescriptionPairList fEntryLineList;
}
