using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.DocumentWrappers;
using CusEntryLineCollection = Enterprise.Customs.Business.ICusEntryLineCollection<Enterprise.Customs.FR.Business.Declaration.CusEntryLine>;

namespace Enterprise.Customs.FR.DocumentWrappers.LiquidationDetails;

public class LiquidationDetailsLineWrapperCollection : DocBaseWrapperCollection<LiquidationDetailsLineWrapper>
{
	public LiquidationDetailsLineWrapperCollection(CusEntryLineCollection entryLineCollection, BusinessObjectFactory factory)
		: this(entryLineCollection.Cast<CusEntryLine>(), factory)
	{
	}

	public LiquidationDetailsLineWrapperCollection(IEnumerable<CusEntryLine> entryLineCollection, BusinessObjectFactory factory)
		: base(factory)
	{
		foreach (var entryLine in entryLineCollection)
		{
			Add(LiquidationDetailsLineWrapper.New(entryLine, factory));
		}
	}
}
