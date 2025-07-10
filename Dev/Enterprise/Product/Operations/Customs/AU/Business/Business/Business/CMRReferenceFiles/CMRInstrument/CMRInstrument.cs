
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[CodeProperty(CMRInstrument.Schema.IN_Number), DescriptionProperty(CMRInstrument.Schema.IN_Number)]
	public class CMRInstrument : AutoCMRInstrument
	{
		public new class Schema : AutoCMRInstrument.Schema
		{
			public const string TariffGroupRelevant = "TariffGroupRelevant";
		}

		public CMRInstrument(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public const string InstrumentValidationType = "MUST BE";

		public static CMRInstrument New(BusinessObjectFactory factory)
		{
			return factory.New<CMRInstrument>();
		}

		public static CMRInstrument Load(BusinessObjectFactory factory, ZString instrumentNumber)
		{
			return factory.LoadTop1<CMRInstrument>(new ZQuery(CMRInstrumentSchema.IN_Number, instrumentNumber));
		}

		public bool IsValidForThisDate(ZDateTime lodgementDate)
		{
			bool result = false;
			if (!lodgementDate.IsEmpty)
			{
				result = !IN_StartDate.IsEmpty && IN_StartDate <= lodgementDate &&
					(IN_EndDate.IsEmpty || IN_EndDate >= lodgementDate) &&
					(IN_RevocationDate.IsEmpty || IN_RevocationDate >= lodgementDate);
			}
			return result;
		}

		public bool IsValidForThisTariff(ZString tariffAndStatNumber)
		{
			bool result = false;

			if (IN_TariffValidationType == InstrumentValidationType)
			{
				result = InstrumentTariffCollection.HasThisTariffAndStatNumber(tariffAndStatNumber);
			}
			else if (IN_TariffValidationType.IsEmpty)
			{
				result = true;
			}
			return result;
		}

		public ZString TariffGroupRelevant
		{
			get
			{
				if (!isTariffGroupRelevantCalculated)
				{
					ZStringBuilder result = new ZStringBuilder();
					foreach (CMRInstrumentTariffGroup instrumentTariffGroup in InstrumentTariffCollection)
					{
						result.Append(instrumentTariffGroup.IG_TariffGroupItem + ",");
					}
					fTariffGroupRelevant = result.ToString().TrimEnd(',');
					isTariffGroupRelevantCalculated = true;
				}
				return fTariffGroupRelevant;
			}
		}
		ZString fTariffGroupRelevant;
		bool isTariffGroupRelevantCalculated;

		public ZPropertyInfo TariffGroupRelevantInfo
		{
			get { return GetZPropertyInfo(Schema.TariffGroupRelevant); }
		}

		CMRInstrumentTariffGroupCollection InstrumentTariffCollection
		{
			get
			{
				if (fInstrumentTariffCollection == null)
				{
					ZQuery filter = new ZQuery(CMRInstrumentTariffGroupSchema.IG_InstrumentNumber, IN_Number);
					filter.AddToFilter(JoinCondition.And, CMRInstrumentTariffGroupSchema.IG_InstrumentType, SQLComparisonOperator.Equal, IN_Type);
					fInstrumentTariffCollection = new CMRInstrumentTariffGroupCollection(Factory, filter);
					fInstrumentTariffCollection.Load();
				}
				return fInstrumentTariffCollection;
			}
		}
		CMRInstrumentTariffGroupCollection fInstrumentTariffCollection;
	}
}
