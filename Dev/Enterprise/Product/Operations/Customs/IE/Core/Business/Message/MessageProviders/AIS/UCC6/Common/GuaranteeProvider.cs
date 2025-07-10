using System.Collections.Generic;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class GuaranteeProvider : IGuarantee
	{
		public GuaranteeProvider(List<GuaranteeForEntryInstruction> guaranteeListForType, ZString type, ZShort sequenceNumber)
		{
			this.guaranteeListForType = guaranteeListForType;

			this.sequenceNumber = sequenceNumber.ToString();
			guaranteeType = type;
		}

		readonly string sequenceNumber;
		readonly string guaranteeType;

		readonly List<GuaranteeForEntryInstruction> guaranteeListForType;

		public string SequenceNumber => sequenceNumber;

		public string GuaranteeType => guaranteeType;

		public IReadOnlyCollection<IGuaranteeReference> GuaranteeReference
		{
			get
			{
				if (guaranteeReference == null)
				{
					guaranteeReference = new List<IGuaranteeReference>();

					ZShort sequenceNumber = 1;
					foreach (var guarantee in guaranteeListForType)
					{
						guaranteeReference.Add(new GuaranteeReferenceProvider(guarantee, sequenceNumber));
						sequenceNumber++;
					}
				}
				return guaranteeReference;
			}
		}

		List<IGuaranteeReference> guaranteeReference;
	}
}
