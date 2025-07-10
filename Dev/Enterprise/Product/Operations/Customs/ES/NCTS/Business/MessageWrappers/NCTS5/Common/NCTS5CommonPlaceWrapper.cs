using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonPlaceWrapper : INCTSCommonPlace
	{
		public NCTS5CommonPlaceWrapper(ZString placeCode, BusinessObjectFactory factory)
		{
			var unloco = ZString.Empty;
			var country = placeCode.SubstringSafe(0, 2);
			var location = placeCode.SubstringSafe(2);

			if (placeCode.Length == UNLOCOLength)
			{
				var filter = new RefUNLOCOCollection(factory).CompleteFilter;
				filter.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.Equal, placeCode);
				var result = factory.LoadTop1<RefUNLOCO>(filter);
				if (result != null)
				{
					unloco = placeCode;
					country = ZString.Empty;
					location = ZString.Empty;
				}
			}

			UNLocode = unloco;
			Country = country;
			Location = location;
		}

		const int UNLOCOLength = 5;

		public ZString UNLocode { get; }

		public ZString Country { get; }

		public ZString Location { get; }
	}
}
