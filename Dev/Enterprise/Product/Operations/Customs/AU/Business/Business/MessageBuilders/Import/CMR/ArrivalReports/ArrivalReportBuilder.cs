using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSREP;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// Summary description for ImpendingArrivalReportBuilder.
	/// </summary>
	public abstract class ArrivalReportBuilder : CMRCUSREPMessageBuilder
	{
		public ArrivalReportBuilder(IArrivalReportInformation reportInfo)
		{
			reportInformation = reportInfo;
		}

		protected internal override DocumentNameCodeList DocumentNameCode => DocumentNameCodeList.ArrivalInformation;

		protected internal override void GenerateMessageText()
		{
			if (cUSREP == null)
			{
				cUSREP = new CUSREPMessage();
				PopulateUNH();
				PopulateBGM();
				PopulateGroup1();
				PopulateGroup2();
				PopulateGroup5();
				PopulateSegmentGroup8();
				PopulateUNT();
			}
		}

		protected void PopulateGroup1()
		{
			if (MessageSubType != Enterprise.Customs.Common.MessageBuilders.MessageSubTypes.Withdraw)
			{
				ZString responsiblePartyClientID = reportInformation.ResponsiblePartyID;
				if (!responsiblePartyClientID.IsEmpty)
				{
					MessageUtilities.PopulateRFF(cUSREP.Group1.InstantiateAChildAndAddItToChildrenCollection().RFF[0], ReferenceFunctionCodeQualifierList.DeclarantsCustomsIdentityNumber, responsiblePartyClientID, null);
				}
			}
		}

		protected void PopulateGroup2()
		{
			PopulateGroup2LOCs();
			PopulateGroup2DTMSegments();
		}

		protected virtual void PopulateGroup2LOCs()
		{
		}

		protected virtual void PopulateGroup2DTMSegments()
		{
		}

		protected virtual void PopulateGroup5()
		{
		}

		protected void PopulateSegmentGroup8()
		{
			PopulateTDTSegment();
			if (cUSREP.Group8.Count > 0)
			{
				PopulateSegmentGroup9();
			}
		}

		protected abstract void PopulateTDTSegment();

		protected virtual void PopulateSegmentGroup9()
		{
		}

		readonly IArrivalReportInformation reportInformation;
	}
}
