using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.JP.AFR.DataTransfer.Universal
{
	public class JPAFRBillsDataObjectWriter : TopLevelDataObjectWriter<JPAFRBills, Shipment>
	{
		public JPAFRBillsDataObjectWriter(IDataWritingManager manager, JPManifestDataObjectWriterHelper helper)
			: base(manager)
		{
			this.helper = helper;
		}

		readonly JPManifestDataObjectWriterHelper helper;

		protected override void PopulateDataObject(JPAFRBills billBO, Shipment billData)
		{
			billData.WayBillNumber = billBO.JPB_BillNumber;
			billData.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House, Description = WayBillTypeList.Descriptions.House };

			var refUNLOCOList = billBO.Factory.GetRefUNLOCOList();
			billData.PortOfOrigin = ListHelper.GetWithName(billBO.JPB_RL_NKOrigin, refUNLOCOList);
			billData.PortOfDestination = ListHelper.GetWithName(billBO.JPB_RL_NKFinalDestination, refUNLOCOList);
			var packingLineData = new PackingLine(writeManager.WriterStrategy)
			{
				DetailedDescription = billBO.JPB_GoodsDescription,
				HarmonisedCode = billBO.JPB_Tariff,
				MarksAndNos = billBO.JPB_MarksAndNumbers,
				PackQty = new ZLong(billBO.JPB_ManifestQty),
				PackType = ListHelper.GetWithDescription<PackageType>(billBO.JPB_ManifestUQ, billBO.Lookups.ManifestUnitList),
				Weight = billBO.JPB_GrossWeight,
				WeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(billBO.JPB_GrossWeightUQ, billBO.Lookups.WeightUnitList),
				Volume = billBO.JPB_Volume,
				VolumeUnit = ListHelper.GetWithDescription<UnitOfVolume>(billBO.JPB_VolumeUQ, billBO.Lookups.VolumeUnitList),
				CountryOfOrigin = Country.New(billBO.GoodsOrigin)
			};

			packingLineData.SetUNDGCollection(() =>
			{
				var result = new List<UNDG>();

				var substance = billBO.UNDGSubstance;
				if (substance != null)
				{
					result.Add(BuildUNDGElement(substance));
				}
				else
				{
					var substanceCode = billBO.JPB_DG_NKSubstance;
					if (!substanceCode.IsEmpty)
					{
						result.Add(new UNDG(writeManager.WriterStrategy) { UNDGCode = substanceCode, Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO });
					}
				}

				var extraUNDGs = billBO.UNDGs.Select(c => c.UNDGSubstance).Where(c => c != null);
				result.AddRange(extraUNDGs.Select(BuildUNDGElement));

				return result.Count > 0 ? result : null;
			});

			billData.SetPackingLineCollection(() => new DataObjectList<PackingLine>(new[] { packingLineData }) { Content = CollectionContent.Complete });

			if (!billBO.JPB_FreightValue.IsEmpty || billBO.JPB_RX_NKFreightValueCurrency.IsEmpty)
			{
				billData.CommercialInfo = new UniversalXml.Customs.CommercialInfo()
				{
					CommercialChargeCollection = new List<UniversalXml.Customs.CommercialCharge>(new[]
					{
						new UniversalXml.Customs.CommercialCharge()
						{
							ChargeType = ListHelper.GetWithDescription<CodeDescriptionPair>(CustomsChargeTypeList.Codes.OverseasFreight, helper.CustomsChargeTypeList),
							Currency = Currency.New(billBO.FreightValueCurrency),
							Amount = billBO.JPB_FreightValue
						}
					})
				};
			}
			billData.SetNoteCollection(() => new DataObjectList<Note>(new[]
			{
				new Note()
				{
					Description = AddInfoConstants.Bill.Remarks,
					IsCustomDescription = ZBool.True,
					NoteText = billBO.JPB_Remarks
				}
			}));
			billData.SetOrganizationAddressCollection(() => ProcessCollection(billBO.DocAddresses, new JobDocAddressDataObjectWriter(writeManager)));
			billData.SetContainerCollection(() => ProcessCollection(billBO.Containers, new JPAFRContainerDataObjectWriter(writeManager), CollectionContent.Complete));
			PopulateAddInfosAndInBondData(billBO, billData);
		}

		UNDG BuildUNDGElement(UNDGSubstance substance)
		{
			return new UNDG(writeManager.WriterStrategy)
			{
				IMOClass = substance.DG_Class,
				PackingGroup = substance.DG_PG,
				ProperShippingName = substance.DG_PSN,
				UNDGCode = substance.DG_Code,
				SubLabel1 = substance.DG_SubLabel1,
				SubLabel2 = substance.DG_SubLabel2,
				Standard = substance.DG_Standard,
			};
		}

		void PopulateAddInfosAndInBondData(JPAFRBills billBO, Shipment billData)
		{
			billData.SetAddInfoCollection(() =>
				{
					var list = new List<AddInfo>();

					var parties = billBO.NotificationForwardingParties.GetNonEmptyInSortOrder();

				#region Notification Forwarding Party

				var partiesLength = parties.Length;
					if (partiesLength > 0)
					{
						list.Add(new AddInfo()
						{
							Key = AddInfoConstants.Bill.NotificationForwardingPartyCode1,
							Value = parties[0].CY_Data
						});
					}
					if (partiesLength > 1)
					{
						list.Add(new AddInfo()
						{
							Key = AddInfoConstants.Bill.NotificationForwardingPartyCode2,
							Value = parties[1].CY_Data
						});
					}
					if (partiesLength > 2)
					{
						list.Add(new AddInfo()
						{
							Key = AddInfoConstants.Bill.NotificationForwardingPartyCode3,
							Value = parties[2].CY_Data
						});
					}

				#endregion

				#region Other Relevant Law

				var laws = billBO.OtherRelevantLaws.GetNonEmptyInSortOrder();
					var lawsLength = laws.Length;
					if (lawsLength > 0)
					{
						list.Add(new AddInfo()
						{
							Key = AddInfoConstants.Bill.OtherRelevantLawCode1,
							Value = laws[0].CY_Data
						});
					}
					if (lawsLength > 1)
					{
						list.Add(new AddInfo()
						{
							Key = AddInfoConstants.Bill.OtherRelevantLawCode2,
							Value = laws[1].CY_Data
						});
					}
					if (lawsLength > 2)
					{
						list.Add(new AddInfo()
						{
							Key = AddInfoConstants.Bill.OtherRelevantLawCode3,
							Value = laws[2].CY_Data
						});
					}
					if (lawsLength > 3)
					{
						list.Add(new AddInfo()
						{
							Key = AddInfoConstants.Bill.OtherRelevantLawCode4,
							Value = laws[3].CY_Data
						});
					}
					if (lawsLength > 4)
					{
						list.Add(new AddInfo()
						{
							Key = AddInfoConstants.Bill.OtherRelevantLawCode5,
							Value = laws[4].CY_Data
						});
					}

				#endregion

				#region Place Of Delivery

				var placeOfDeliveryCode = ZString.Empty;
					var placeOfDeliveryName = ZString.Empty;
					var delivery = UNLOCO.New(billBO.Delivery);
					if (delivery != null)
					{
						placeOfDeliveryCode = delivery.Code.GetValueOrDefault();
						placeOfDeliveryName = delivery.Name.GetValueOrDefault();
					}
					list.Add(new AddInfo()
					{
						Key = AddInfoConstants.Bill.PlaceOfDeliveryCode,
						Value = placeOfDeliveryCode
					});
					list.Add(new AddInfo()
					{
						Key = AddInfoConstants.Bill.PlaceOfDeliveryName,
						Value = placeOfDeliveryName
					});

				#endregion

				#region VOCC Section

				var isBillShippingLineEntry = billBO.IsShippingLineEntry;
					if (isBillShippingLineEntry)
					{
						list.Add(new AddInfo()
						{
							Key = AddInfoConstants.Bill.MasterBillIdentifier,
							Value = (billBO.JPB_IsMaterBill ? AddInfoConstants.Bill.MasterBillIdentifierValue : string.Empty)
						});
						list.Add(new AddInfo()
						{
							Key = AddInfoConstants.Bill.ContainerOperatorCode,
							Value = billBO.JPB_ContainerOperatorCode
						});
					}

				#endregion

				#region Delete Reason

				var sendingObject = billBO.Factory.Load<MessageSendingObject>(billBO.PK);
					if (sendingObject != null && sendingObject.IsBillSendAndDelete)
					{
						list.Add(new AddInfo
						{
							Key = AddInfoConstants.Bill.DeleteReasonCode,
							Value = sendingObject.JPM_DeleteReasonCode
						});
						list.Add(new AddInfo
						{
							Key = AddInfoConstants.Bill.DeleteReasonText,
							Value = sendingObject.JPM_DeleteReasonText
						});
					}

				#endregion

				if (!billBO.JPB_SpecialCargoCode.IsEmpty)
					{
						list.Add(new AddInfo
						{
							Key = AddInfoConstants.Bill.SpecialCargoCode,
							Value = billBO.JPB_SpecialCargoCode
						});
					}

					PopulateInBondDetailsData(list, billData, billBO.InBondDetails, isBillShippingLineEntry);
					return list;
				});
		}

		void PopulateInBondDetailsData(List<AddInfo> list, Shipment billData, JPAFRInBondDetails inBondDetailsBO, bool isBillShippingLineEntry)
		{
			var arrivalBondedAreaCode = ZString.Empty;
			var arrivalBondedAreaName = ZString.Empty;
			var eFDT = ZDateTime.Empty;
			var eSDT = ZDateTime.Empty;
			var goodsValue = ZDecimal.Zero;
			Currency goodsValueCurrency = null;
			var temporaryLandingDuration = ZInt.Zero;
			var temporaryLandingReason = ZString.Empty;
			var transportMode = ZString.Empty;
			var generalCustomsTransitApprovalNumber = ZString.Empty;
			if (inBondDetailsBO != null)
			{
				arrivalBondedAreaCode = inBondDetailsBO.JPI_ArrivalBondedAreaCode;
				arrivalBondedAreaName = ZString.Empty; // TODO: Add Name
				eFDT = inBondDetailsBO.JPI_EFDT;
				eSDT = inBondDetailsBO.JPI_ESDT;
				goodsValue = inBondDetailsBO.JPI_GoodsValue;
				goodsValueCurrency = ListHelper.GetWithDescription<Currency>(inBondDetailsBO.JPI_RX_NKGoodsValueCurrency, inBondDetailsBO.Lookups.GoodsValueCurrencies);
				temporaryLandingDuration = inBondDetailsBO.JPI_TemporaryLandingDuration;
				temporaryLandingReason = inBondDetailsBO.JPI_TemporaryLandingReason;
				transportMode = inBondDetailsBO.JPI_TransportMode;
				generalCustomsTransitApprovalNumber = inBondDetailsBO.JPI_GeneralCustomsTransitApprovalNumber;
			}
			billData.GoodsValue = goodsValue;
			billData.GoodsValueCurrency = goodsValueCurrency;
			list.Add(new AddInfo()
			{
				Key = AddInfoConstants.Bill.TranshipmentArrivalPlaceCode,
				Value = arrivalBondedAreaCode
			});
			list.Add(new AddInfo()
			{
				Key = AddInfoConstants.Bill.TranshipmentArrivalPlaceName,
				Value = arrivalBondedAreaName
			});
			list.Add(new AddInfo()
			{
				Key = AddInfoConstants.Bill.TranshipmentEstimatedStartDate,
				Value = eSDT.ToISO8601String()
			});
			list.Add(new AddInfo()
			{
				Key = AddInfoConstants.Bill.TranshipmentEstimatedFinishDate,
				Value = eFDT.ToISO8601String()
			});
			list.Add(new AddInfo()
			{
				Key = AddInfoConstants.Bill.TranshipmentDuration,
				Value = temporaryLandingDuration.ToString()
			});
			list.Add(new AddInfo()
			{
				Key = AddInfoConstants.Bill.TranshipmentReasonCode,
				Value = temporaryLandingReason
			});
			list.Add(new AddInfo()
			{
				Key = AddInfoConstants.Bill.TranshipmentTransportMode,
				Value = transportMode
			});
			if (isBillShippingLineEntry)
			{
				list.Add(new AddInfo()
				{
					Key = AddInfoConstants.Bill.GeneralCustomsTransitApprovalNumber,
					Value = generalCustomsTransitApprovalNumber
				});
			}
		}

		protected override ZString GetEDIMessageSubType()
		{
			return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.AFRBill;
		}
	}
}
