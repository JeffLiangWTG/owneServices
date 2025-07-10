using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class DummyWithCodeDescriptionPairList : DummyBusinessObject
	{
		public DummyWithCodeDescriptionPairList(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ReadOnlyCodeDescriptionPairList fDummyList;

		public virtual ReadOnlyCodeDescriptionPairList DummyList
		{
			get
			{
				var accessedToVerifyNotDeleted = Z0_Code;
				if (fDummyList == null)
				{
					var list = new CodeDescriptionPairList();

					list.AddPair(Guid.NewGuid(), "ONE", "Description 01");
					list.AddPair(Guid.NewGuid(), "TWO", "Description 02");
					list.AddPair(Guid.NewGuid(), "THREE", "Description 03");
					list.AddPair(Guid.NewGuid(), "FOUR", "Description 04");
					list.AddPair(Guid.NewGuid(), "FIVE", "Description 05");
					list.AddPair(Guid.NewGuid(), "SIX", "Description 06");
					list.AddPair(Guid.NewGuid(), "SEVEN", "Description 07");
					list.AddPair(Guid.NewGuid(), "EIGHT", "Description 08");
					list.AddPair(Guid.NewGuid(), "NINE", "Description 09");
					list.AddPair(Guid.NewGuid(), "TEN", "Description 10");
					list.AddPair(Guid.NewGuid(), "ELEVN", "Description 11");
					list.AddPair(Guid.NewGuid(), "TWELV", "Description 12");
					list.AddPair(Guid.NewGuid(), "THRTN", "Description 13");
					list.AddPair(Guid.NewGuid(), "FORTN", "Description 14");
					list.AddPair(Guid.NewGuid(), "FIFTN", "Description 15");

					fDummyList = new ReadOnlyCodeDescriptionPairList(list);
				}

				return fDummyList;
			}
		}

		protected override DummyBizoValidation GetNewValidation()
		{
			return new DummyWithCodeDescriptionPairListValidation(this);
		}
	}
}
