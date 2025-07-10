using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class DummyWithChangingCodeDescriptionPairList : DummyWithCodeDescriptionPairList
	{
		public DummyWithChangingCodeDescriptionPairList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void SetList(bool isList1)
		{
			this.IsList1 = isList1;
			Z0_FK_Code = isList1 ? "ONE" : "EIGHT";
		}

		bool IsList1;

		public override ReadOnlyCodeDescriptionPairList DummyList
		{
			get
			{
				var list = new CodeDescriptionPairList();

				if (IsList1)
				{
					list.AddPair(Guid.NewGuid(), "ONE", "Description 01");
					list.AddPair(Guid.NewGuid(), "TWO", "Description 02");
					list.AddPair(Guid.NewGuid(), "THREE", "Description 03");
					list.AddPair(Guid.NewGuid(), "FOUR", "Description 04");
					list.AddPair(Guid.NewGuid(), "FIVE", "Description 05");
					list.AddPair(Guid.NewGuid(), "SIX", "Description 06");
					list.AddPair(Guid.NewGuid(), "SEVEN", "Description 07");
				}
				else
				{
					list.AddPair(Guid.NewGuid(), "EIGHT", "Description 08");
					list.AddPair(Guid.NewGuid(), "NINE", "Description 09");
					list.AddPair(Guid.NewGuid(), "TEN", "Description 10");
					list.AddPair(Guid.NewGuid(), "ELEVN", "Description 11");
					list.AddPair(Guid.NewGuid(), "TWELV", "Description 12");
					list.AddPair(Guid.NewGuid(), "THRTN", "Description 13");
					list.AddPair(Guid.NewGuid(), "FORTN", "Description 14");
					list.AddPair(Guid.NewGuid(), "FIFTN", "Description 15");
				}

				return new ReadOnlyCodeDescriptionPairList(list);
			}
		}
	}
}
