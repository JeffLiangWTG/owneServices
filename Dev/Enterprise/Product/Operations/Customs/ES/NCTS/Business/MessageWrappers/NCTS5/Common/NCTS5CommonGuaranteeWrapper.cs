using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonGuaranteeWrapper : INCTSCommonGuarantee
	{
		public NCTS5CommonGuaranteeWrapper(List<NctsGuarantee> guaranteeListForType, ZString type, ZShort seqNum)
		{
			this.guaranteeListForType = guaranteeListForType;

			SequenceNumber = seqNum.ToString();
			GuaranteeType = type;
		}

		readonly List<NctsGuarantee> guaranteeListForType;

		public ZString SequenceNumber { get; }

		public ZString GuaranteeType { get; }

		public IReadOnlyCollection<INCTSCommonGuaranteeReference> GuaranteeReference
		{
			get
			{
				if (guaranteeReference == null)
				{
					var guaranteeReferenceList = new List<NCTS5CommonGuaranteeReferenceWrapper>();

					if (GuaranteeType != EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.MovementsCarriedUnderTheTirConvention)
					{
						ZShort seqNum = 1;
						foreach (var guarantee in guaranteeListForType)
						{
							guaranteeReferenceList.Add(new NCTS5CommonGuaranteeReferenceWrapper(guarantee, seqNum));
							seqNum++;
						}
					}
					guaranteeReference = guaranteeReferenceList.AsReadOnly();
				}
				return guaranteeReference;
			}
		}
		IReadOnlyCollection<NCTS5CommonGuaranteeReferenceWrapper> guaranteeReference;

		public static IReadOnlyCollection<NCTS5CommonGuaranteeWrapper> GetGuaranteeList(NctsHeader header)
		{
			var groupedGuarantees = header.GetEffectiveGuarantees().Cast<NctsGuarantee>().GroupBy(x => x.PW_BondType);

			var guarantees = new List<NCTS5CommonGuaranteeWrapper>();
			ZShort seqNum = 1;
			foreach (var groupedGuarantee in groupedGuarantees)
			{
				guarantees.Add(new NCTS5CommonGuaranteeWrapper(groupedGuarantee.ToList(), groupedGuarantee.Key, seqNum));
				seqNum++;
			}
			return guarantees.AsReadOnly();
		}
	}
}
