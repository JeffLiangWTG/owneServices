using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class DepotMessageBuilder : CMRCUSCARMessageBuilder
	{
		public DepotMessageBuilder(ZString cAN)
		{
			this.cAN = cAN;
		}

		protected internal override void GenerateMessageText()
		{
			if (CUSCAR == null)
			{
				CUSCAR = new CUSCARMessage();
				PopulateUNH();
				PopulateBGM();
				PopulateLOCs();
				PopulateSegmentGroup1();
				PopulateUNT();
			}
		}

		#region Implementation

		protected abstract void PopulateLOCs();

		protected void PopulateSegmentGroup1()
		{
			if (!cAN.IsEmpty)
			{
				MessageUtilities.PopulateRFF(CUSCAR.Group1[0].RFF[0], ReferenceFunctionCodeQualifierList.TransactionReferenceNumber, cAN, null);
			}
		}

		protected ZString cAN;

		#endregion
	}
}
