using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;
using Extensions = Enterprise.Customs.DataTransfer.Universal.Extensions;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.EMCS.DataTransfer
{
	public class EMCSDeclarationDataObjectWriter : Customs.DataTransfer.Universal.DeclarationDataObjectWriter
	{
		public EMCSDeclarationDataObjectWriter(IDataWritingManager manager) : base(manager)
		{
		}

		protected override Customs.DataTransfer.Universal.UniversalDataObjectWriterHelper CreateNewUniversalDataObjectWriterHelper(BaseJobDeclaration declarationBO)
		{
			return new EMCSUniversalDataObjectWriterHelper(declarationBO.Factory, declarationBO.CountryCode);
		}

		protected override Customs.DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter GetNewCommercialInvoiceHeaderDataObjectWriter(CusEntryHeader relatedEntry)
		{
			return new EMCSCommercialInvoiceHeaderDataObjectWriter(writeManager, helper, landedCostDataWriter, relatedEntry);
		}

		protected override List<AddInfo> CreateDeclarationAddInfo(BaseJobDeclaration declarationBO, UniversalShipment declarationData)
		{
			var addInfos = base.CreateDeclarationAddInfo(declarationBO, declarationData);
			var sourceBO = (EMCSJobDeclaration)declarationBO;
			Extensions.AddOrUpdate(addInfos, Constants.AddInfo.Keys.JourneyTime, sourceBO.JourneyTimeNumericPart.ToString() + sourceBO.JourneyTimeFormatPart);
			return addInfos;
		}

		protected override void PopulateContainerAddInfo(Container containerData, BaseCusContainer containerBO)
		{
			base.PopulateContainerAddInfo(containerData, containerBO);
			var addInfos = containerData.AddInfoCollection ?? new List<AddInfo>();
			containerData.SetAddInfoCollection(() => addInfos);

			var bo = (EMCSCusContainer)containerBO;
			Extensions.AddOrUpdate(addInfos, Constants.AddInfo.Keys.SealDetails, bo.SealDetails);
			Extensions.AddOrUpdate(addInfos, Constants.AddInfo.Keys.Comment, bo.Comment);
		}

		protected override void PopulateDates(BaseJobDeclaration declarationBO, UniversalShipment declarationData, bool keepExistingData)
		{
			base.PopulateDates(declarationBO, declarationData, keepExistingData);

			if (!keepExistingData)
			{
				var sourceBO = (EMCSJobDeclaration)declarationBO;
				var dateCollection = declarationData.DateCollection;
				if (dateCollection != null)
				{
					var newDispatchTime = sourceBO.JE_DateAtOrigin;
					var departure = dateCollection.FirstOrDefault(x => x.Type.GetValueOrDefault() == DateType.Departure);
					if (departure != null)
					{
						if (departure.Value != newDispatchTime)
						{
							departure.Value = newDispatchTime;
						}

						if (departure.IsEstimate.GetValueOrDefault(ZBool.True))
						{
							departure.IsEstimate = ZBool.False;
						}
					}
					else
					{
						dateCollection.Add(Date.New(DateType.Departure, ZBool.False, newDispatchTime));
					}
				}
			}
		}
	}
}
