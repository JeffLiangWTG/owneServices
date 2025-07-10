using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class LocalTransportLegWrapper : GenericWrapper
	{
		#region Constructor

		public LocalTransportLegWrapper(CommonCartageLeg leg, BusinessObjectFactory factory)
			: base(leg ?? factory.GetNull<CommonCartageLeg>(), factory)
		{
		}

		#endregion

		#region Properties

		#region Client

		public OrganisationWrapper Client
		{
			get { return new OrganisationWrapper(OrganisationUsageType.Client, LegBO.Client, ContactType.LocalClient, Factory); }
		}

		#endregion

		#region DeliverySignedFor

		public ZString DeliverySignedFor
		{
			get { return LegBO.JU_DeliverySignedFor; }
		}

		#endregion

		#region DeliveryTimeIn

		public ZDateTime DeliveryTimeIn
		{
			get { return LegBO.JU_DeliverTimeIn; }
		}

		#endregion

		#region DeliveryTimeOut

		public ZDateTime DeliveryTimeOut
		{
			get { return LegBO.JU_DeliverTimeOut; }
		}

		#endregion

		#region DeliveryTimeDemurrage

		public ZString DeliveryTimeDemurrage
		{
			get
			{
				var deliveryDemurrage = LegBO.GetDemurrageIncludingFreeTime(LegBO.JU_CartageDeliveryDemurrage, LegBO.DeliverToDocAddress);
				return new TotalHoursHelper().GetTextFromTimeSpan(deliveryDemurrage);
			}
		}

		#endregion

		#region DeliveryReadyHeading

		public ZString DeliveryReadyHeading
		{
			get
			{
				ZString result = "";
				CommonBookedCtgMove bookedMove = BookedMoveBO;

				if (LegBO.DeliverToDocAddressType == DocAddressType.LocalCartageCTO && Container != null && !Container.DepartureSlotTime.IsEmpty)
				{
					result = Res.GetString("b64d8068-1da6-45bf-b48f-9c84fc292b02", "SLOT:");
				}
				else if (IsDeliveryTheRequestedBooking && bookedMove != null && !bookedMove.EW_RequestedDeliveryTimeStart.IsEmpty)
				{
					result = Res.GetString("33ca4281-a147-48d8-9b93-e91406869a10", "READY FROM:");
				}
				else if (DeliverTo != null && !DeliverTo.DeliverFromTime.IsEmpty)
				{
					result = Res.GetString("f6c2fcd5-f7ae-4134-99bc-a17d2dfc32dc", "OPEN:");
				}

				return result;
			}
		}

		#endregion

		#region DeliveryReady

		public ZString DeliveryReady
		{
			get
			{
				ZString result = "";
				var bookedMove = BookedMoveBO;

				if (LegBO.DeliverToDocAddressType == DocAddressType.LocalCartageCTO && Container != null && !Container.DepartureSlotTime.IsEmpty)
				{
					result = Container.DepartureSlotTime.ToLongTimeString();
				}
				else if (IsDeliveryTheRequestedBooking && bookedMove != null && !bookedMove.EW_RequestedDeliveryTimeStart.IsEmpty)
				{
					result = bookedMove.EW_RequestedDeliveryTimeStart.ToLongTimeString();
				}
				else if (DeliverTo != null)
				{
					result = DeliverTo.DeliverFromTime;
				}

				return result;
			}
		}

		#endregion

		#region DeliveryCloseHeading

		public ZString DeliveryCloseHeading
		{
			get
			{
				ZString result = "";

				CommonBookedCtgMove bookedMove = BookedMoveBO;
				if (IsDeliveryTheRequestedBooking && bookedMove != null && !bookedMove.EW_RequestedDeliveryTimeEnd.IsEmpty)
				{
					result = Res.GetString("2527613f-0751-4a67-895f-09ee336c5f54", "READY TO:");
				}
				else if (DeliverTo != null && !DeliverTo.DeliverToTime.IsEmpty)
				{
					result = Res.GetString("e8838002-a908-47ac-b813-005ec88b12a6", "CLOSE:");
				}

				return result;
			}
		}

		#endregion

		#region DeliveryClose

		public ZString DeliveryClose
		{
			get
			{
				ZString result = "";
				var bookedMove = BookedMoveBO;

				if (IsDeliveryTheRequestedBooking && bookedMove != null && !bookedMove.EW_RequestedDeliveryTimeEnd.IsEmpty)
				{
					result = bookedMove.EW_RequestedDeliveryTimeEnd.ToLongTimeString();
				}
				else if (DeliverTo != null && !DeliverTo.DeliverToTime.IsEmpty)
				{
					result = DeliverTo.DeliverToTime;
				}

				return result;
			}
		}

		#endregion

		#region HasWaitPoint

		public ZBool HasWaitPoint
		{
			get { return LegBO.HasWaitPoint; }
		}

		#endregion

		#region UNDGSubstances

		public UNDGSubstanceWrapperCollection UNDGSubstances
		{
			get
			{
				var result = new UNDGSubstanceWrapperCollection(Factory);
				if (BookedMoveBO != null)
				{
					foreach (UNDGDataItem dgItem in BookedMoveBO.UNDGs)
					{
						if (dgItem.Substance != null)
						{
							result.Add(new UNDGSubstanceWrapper(dgItem, Factory));
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region IsDeliveryTheRequestedBooking

		public ZBool IsDeliveryTheRequestedBooking
		{
			get
			{
				CommonBookedCtgMove bookedMove = BookedMoveBO;
				return bookedMove != null && bookedMove.RequestedAddressCode == CommonCartageAddressHelper.GetOrgTypeFromCartageDocAddressType(LegBO.DeliverToDocAddressType);
			}
		}

		#endregion

		#region IsLoose

		public ZBool IsLoose
		{
			get { return LegBO.IsLoose; }
		}

		#endregion

		#region IsPickupTheRequestedBooking

		public ZBool IsPickupTheRequestedBooking
		{
			get
			{
				CommonBookedCtgMove bookedMove = BookedMoveBO;
				return bookedMove != null && bookedMove.RequestedAddressCode == CommonCartageAddressHelper.GetOrgTypeFromCartageDocAddressType(LegBO.PickupDocAddressType);
			}
		}

		#endregion

		#region IsWaitPointTheRequestedBooking

		public ZBool IsWaitPointTheRequestedBooking
		{
			get
			{
				CommonBookedCtgMove bookedMove = BookedMoveBO;
				return bookedMove != null && bookedMove.RequestedAddressCode == CommonCartageAddressHelper.GetOrgTypeFromCartageDocAddressType(LegBO.WaitPointDocAddressType);
			}
		}

		#endregion

		#region IsPickupCTO

		public ZBool IsPickupCTO
		{
			get { return LegBO.PickupDocAddressType == DocAddressType.LocalCartageCTO; }
		}

		#endregion

		#region IsPickupContainerYard

		public ZBool IsPickupContainerYard
		{
			get { return LegBO.PickupDocAddressType == DocAddressType.LocalCartageYard; }
		}

		#endregion

		#region IsDeliveryCTO

		public ZBool IsDeliveryCTO
		{
			get { return LegBO.DeliverToDocAddressType == DocAddressType.LocalCartageCTO; }
		}

		#endregion

		#region LegNotes

		public ZString LegNotes
		{
			get { return LegBO.JU_LegNotes; }
		}

		#endregion

		#region PickupReadyHeading

		public ZString PickupReadyHeading
		{
			get
			{
				ZString result = "";
				CommonBookedCtgMove bookedMove = BookedMoveBO;

				if (LegBO.PickupDocAddressType == DocAddressType.LocalCartageCTO && Container != null && !Container.ArrivalSlotTime.IsEmpty)
				{
					result = Res.GetString("b64d8068-1da6-45bf-b48f-9c84fc292b02", "SLOT:");
				}
				else if (IsPickupTheRequestedBooking && bookedMove != null && !bookedMove.EW_RequestedPickupTimeStart.IsEmpty)
				{
					result = Res.GetString("33ca4281-a147-48d8-9b93-e91406869a10", "READY FROM:");
				}
				else if (PickupFrom != null && !PickupFrom.PickupFromTime.IsEmpty)
				{
					result = Res.GetString("f6c2fcd5-f7ae-4134-99bc-a17d2dfc32dc", "OPEN:");
				}

				return result;
			}
		}

		#endregion

		#region PickupReady

		public ZString PickupReady
		{
			get
			{
				ZString result = "";
				var bookedMove = BookedMoveBO;

				if (LegBO.PickupDocAddressType == DocAddressType.LocalCartageCTO && Container != null && !Container.ArrivalSlotTime.IsEmpty)
				{
					result = Container.ArrivalSlotTime.ToLongTimeString();
				}
				else if (IsPickupTheRequestedBooking && bookedMove != null && !bookedMove.EW_RequestedPickupTimeStart.IsEmpty)
				{
					result = bookedMove.EW_RequestedPickupTimeStart.ToLongTimeString();
				}
				else if (PickupFrom != null)
				{
					result = PickupFrom.PickupFromTime;
				}

				return result;
			}
		}

		#endregion

		#region PickupCloseHeading

		public ZString PickupCloseHeading
		{
			get
			{
				ZString result = "";

				CommonBookedCtgMove bookedMove = BookedMoveBO;
				if (IsPickupTheRequestedBooking && bookedMove != null && !bookedMove.EW_RequestedPickupTimeEnd.IsEmpty)
				{
					result = Res.GetString("2527613f-0751-4a67-895f-09ee336c5f54", "READY TO:");
				}
				else if (PickupFrom != null && !PickupFrom.PickupToTime.IsEmpty)
				{
					result = Res.GetString("e8838002-a908-47ac-b813-005ec88b12a6", "CLOSE:");
				}

				return result;
			}
		}

		#endregion

		#region PickupClose

		public ZString PickupClose
		{
			get
			{
				ZString result = "";
				var bookedMove = BookedMoveBO;

				if (IsPickupTheRequestedBooking && bookedMove != null && !bookedMove.EW_RequestedPickupTimeEnd.IsEmpty)
				{
					result = bookedMove.EW_RequestedPickupTimeEnd.ToLongTimeString();
				}
				else if (PickupFrom != null)
				{
					result = PickupFrom.PickupToTime;
				}

				return result;
			}
		}

		#endregion

		#region PickupTimeIn

		public ZDateTime PickupTimeIn
		{
			get { return LegBO.JU_PickupTimeIn; }
		}

		#endregion

		#region PickupTimeOut

		public ZDateTime PickupTimeOut
		{
			get { return LegBO.JU_PickupTimeOut; }
		}

		#endregion

		#region PickupTimeDemurrage

		public ZString PickupTimeDemurrage
		{
			get
			{
				var pickupDemurrage = LegBO.GetDemurrageIncludingFreeTime(LegBO.JU_CartagePickupDemurrage, LegBO.PickupFromDocAddress);
				return new TotalHoursHelper().GetTextFromTimeSpan(pickupDemurrage);
			}
		}

		#endregion

		#region PlannedPickupTime

		public ZDateTime PlannedPickupTime
		{
			get { return LegBO.JU_PlannedPickupTime; }
		}

		#endregion

		#region PlannedPickupTimeEnd

		public ZDateTime PlannedPickupTimeEnd
		{
			get { return LegBO.JU_PlannedPickupTimeEnd; }
		}

		#endregion

		#region PlannedDeliveryTime

		public ZDateTime PlannedDeliveryTime
		{
			get { return LegBO.JU_EstimatedDeliveryTime; }
		}

		#endregion

		#region PlannedDeliveryTimeEnd

		public ZDateTime PlannedDeliveryTimeEnd
		{
			get { return LegBO.JU_EstimatedDeliveryTimeEnd; }
		}

		#endregion

		#region PlannedWaitPointTime

		public ZDateTime PlannedWaitPointTime
		{
			get { return LegBO.JU_EstimatedDeliveryTime; }
		}

		#endregion

		#region PlannedWaitPointTimeEnd

		public ZDateTime PlannedWaitPointTimeEnd
		{
			get { return LegBO.JU_EstimatedDeliveryTimeEnd; }
		}

		#endregion

		#region Remarks

		public ZString Remarks
		{
			get
			{
				//Max 5 lines ... If this changes, please update LocalCartage Daily WorkSheet.xls

				ZStringBuilder result = new ZStringBuilder();

				if (CartageBO != null)
				{
					bool showContainerNo = LegBO.IsContainerised;
					bool showArrivalSlot = LegBO.IsContainerised && LegBO.PickupDocAddressType == DocAddressType.LocalCartageCTO;
					bool showDepartureSlot = LegBO.IsContainerised && LegBO.DeliverToDocAddressType == DocAddressType.LocalCartageCTO;
					bool showReleaseNum = LegBO.IsContainerised && LegBO.PickupDocAddressType == DocAddressType.LocalCartageYard;

					bool showPackingDetails = LegBO.IsLoose;
					bool showLCLCutOff = LegBO.IsLoose && LegBO.DeliverToDocAddressType == DocAddressType.LocalCartageCFS;

					if (showContainerNo && Container != null)
					{
						result.Append(ContainerDetails); //1 Line

						if (showArrivalSlot && !Container.ArrivalSlotReference.IsEmpty)
						{
							ZStringBuilder arvSlot = new ZStringBuilder();
							arvSlot.Append(Res.GetString("2f466bd3-925a-44e7-8f54-22f18a1a63e1", "SLOT REF: {0}", Container.ArrivalSlotReference));
							if (!Container.ArrivalSlotTime.IsEmpty)
							{
								arvSlot.Append(Container.ArrivalSlotTime.ToLongTimeString());
							}

							result.AppendIfNotEmpty(arvSlot.ToStringWithDelimiterBetweenAppends(" @ "));
						}

						if (showDepartureSlot && !Container.DepartureSlotReference.IsEmpty)
						{
							ZStringBuilder depSlot = new ZStringBuilder();
							depSlot.Append(Res.GetString("2f466bd3-925a-44e7-8f54-22f18a1a63e1", "SLOT REF: {0}", Container.DepartureSlotReference));
							if (!Container.DepartureSlotTime.IsEmpty)
							{
								depSlot.Append(Container.DepartureSlotTime.ToLongTimeString());
							}

							result.AppendIfNotEmpty(depSlot.ToStringWithDelimiterBetweenAppends(" @ "));
						}

						var line3 = new ZStringBuilder();
						line3.AppendIfNotEmpty(DropModeWithCaption);
						if (showReleaseNum)
						{
							line3.AppendIfNotEmpty(FormatCaptionAndValue(Res.GetString("ec5684ef-7637-433c-b214-0287b24e8b33", "RELEASE #"), Container.ReleaseNumber));
						}
						result.AppendIfNotEmpty(line3.ToStringWithDelimiterBetweenAppends("   "));
					}
					else if (showPackingDetails)
					{
						result.AppendIfNotEmpty(LooseDetails); // 2 Lines
						result.AppendIfNotEmpty(JobNumberWithCaption); // 1 Line

						if (showLCLCutOff) //1 Line
						{
							result.AppendIfNotEmpty(FormatCaptionAndValue(Res.GetString("1d8d4697-8e2b-455b-ba6a-6ee1199b9d03", "LCL CUTOFF"), CartageBO.LCLCutOff));
						}
					}

					if (CartageBO != null) // 1 Line
					{
						result.AppendIfNotEmpty(Cartage.GoodsDescription);
					}
				}

				return result.ToStringWithNewLineBetweenAppends().TrimEnd();
			}
		}

		ZString JobNumberWithCaption
		{
			get { return FormatCaptionAndValue(Res.GetString("b262bb55-53ff-4ccf-9090-dde443616f91", "JOB #"), Cartage.JobNumber); }
		}

		ZString DropModeWithCaption
		{
			get
			{
				var caption = Res.GetString("67d9541c-30a9-49f3-90e1-a83e1222897c", "DROP MODE");
				var result = ZString.Empty;

				result = FormatCaptionAndValue(caption, BookedMove.DropMode);
				if (result.IsEmpty)
				{
					result = FormatCaptionAndValue(caption, CartageBO.JJ_DropMode);
				}

				return result;
			}
		}

		ZString FormatCaptionAndValue(ZString caption, ZDateTime value)
		{
			return value.IsEmpty ? "" : (string)FormatCaptionAndValue(caption, value.ToLongTimeString());
		}

		ZString FormatCaptionAndValue(ZString caption, ZString value)
		{
			return value.IsEmpty ? "" : string.Format("{0}: {1}", caption, value);
		}

		ZString FormatWeight(ValueAndUnitWrapper wrapper)
		{
			return new ZString(FormatNumeric(wrapper.Value, 1) + " " + wrapper.Unit.Code).Trim();
		}

		ZString LooseDetails
		{
			get
			{
				//2 lines ... If this changes, please check: public ZString Remarks
				ZStringBuilder packsDims = new ZStringBuilder();
				ZStringBuilder weightVolume = new ZStringBuilder();

				ZStringBuilder result = new ZStringBuilder();

				if (BookedMove != null)
				{
					packsDims.Append(Res.GetString("3a667ef4-d0eb-4395-9155-899b5efec86c", "PACKS: {0} {1}", BookedMove.BookedPackages, BookedMove.BookedPackType));
					if (BookedMove.BookedLength > 0 && BookedMove.BookedWidth > 0 && BookedMove.BookedHeight > 0)
					{
						packsDims.Append(Res.GetString("cdd6e8f0-79e4-4ab2-b26b-c41486f5280b", "{0}x{1}x{2} {3}", BookedMove.BookedLength.ToStringTrimZeros(), BookedMove.BookedWidth.ToStringTrimZeros(), BookedMove.BookedHeight.ToStringTrimZeros(), BookedMove.BookedDimensionUnits));
					}
					result.AppendIfNotEmpty(packsDims.ToStringWithDelimiterBetweenAppends(" @ "));

					if (BookedMove.BookedWeight > 0)
					{
						weightVolume.Append(Res.GetString("b5f62429-d9b7-44c9-b52f-67692d7d801e", "WGT: {0} {1}", BookedMove.BookedWeight.ToStringTrimZeros(), BookedMove.BookedWeightUnit));
					}
					if (BookedMove.BookedVolume > 0)
					{
						weightVolume.Append(Res.GetString("ac5419b4-ab07-4e41-8654-15a0ae3b7615", "VOL: {0} {1}", BookedMove.BookedVolume.ToStringTrimZeros(), BookedMove.BookedVolumeUnit));
					}
					result.AppendIfNotEmpty(weightVolume.ToStringWithDelimiterBetweenAppends(" / "));
				}

				return result.ToStringWithNewLineBetweenAppends().TrimEnd();
			}
		}

		ZString ContainerDetails
		{
			get
			{
				//2 line ... If this changes, please check: public ZString Remarks
				var result = new ZStringBuilder();

				if (Container != null && Container.Type != null)
				{
					result.Append(string.Format("{0} - {1}   {2}", Container.Type.Code, Container.ContainerNo, JobNumberWithCaption));

					var weights = new ZStringBuilder();

					var zero = new WeightWrapper(0, Container.WeightTare.Unit.Code, WeightWrapper.StandardDecimalPlaces, new CodeDescriptionPairList(OLookUpEditType.Weight), Factory);
					var tareWT = FormatWeight(Container.WeightTare);
					var netWT = LegBO.JU_IsEmptyContainer ? FormatWeight(zero) : FormatWeight(Container.WeightGoods);
					var grossWT = LegBO.JU_IsEmptyContainer ? FormatWeight(Container.WeightTare) : FormatWeight(Container.WeightGross);
					weights.Append(Res.GetString("417e9e68-e0bf-4ad5-bb56-2bbb8157ad6a", "TARE: {0}  NET: {1}  GROSS: {2}", tareWT, netWT, grossWT));

					if (LegBO.TotalVolume > 0)
					{
						weights.Append(Res.GetString("ac5419b4-ab07-4e41-8654-15a0ae3b7615", "VOL: {0} {1}", LegBO.TotalVolume, LegBO.TotalVolumeUnit));
					}

					result.Append(weights.ToStringWithDelimiterBetweenAppends("  ").TrimEnd());
				}

				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		#endregion

		#region Sequence

		public ZInt Sequence
		{
			get { return LegBO.JU_RunSheetSequence; }
		}

		#endregion

		#region DisplayOrder

		public ZInt DisplayOrder
		{
			get { return LegBO.JU_DisplayOrder; }
		}

		#endregion

		#region Signature

		public Image Signature
		{
			get { return LegBO == null ? null : LegBO.Signature.Image; }
		}

		#endregion

		#region HasSignature

		public ZBool HasSignature
		{
			get { return LegBO != null && LegBO.Signature.ShowSignature; }
		}

		#endregion

		#region WaitPoint

		public AddressWrapper WaitPoint
		{
			get { return new AddressWrapper(LegBO.WaitPointDocAddress, Factory); }
		}

		#endregion

		#region WaitPointReadyHeading

		public ZString WaitPointReadyHeading
		{
			get
			{
				ZString result = "";
				CommonBookedCtgMove bookedMove = BookedMoveBO;

				if (LegBO.WaitPointDocAddressType == DocAddressType.LocalCartageCTO && Container != null && !Container.ArrivalSlotTime.IsEmpty)
				{
					result = Res.GetString("b64d8068-1da6-45bf-b48f-9c84fc292b02", "SLOT:");
				}
				else if (IsWaitPointTheRequestedBooking && bookedMove != null && !bookedMove.EW_RequestedDeliveryTimeStart.IsEmpty)
				{
					result = Res.GetString("33ca4281-a147-48d8-9b93-e91406869a10", "READY FROM:");
				}
				else if (WaitPoint != null && !WaitPoint.DeliverFromTime.IsEmpty)
				{
					result = Res.GetString("f6c2fcd5-f7ae-4134-99bc-a17d2dfc32dc", "OPEN:");
				}

				return result;
			}
		}

		#endregion

		#region WaitPointReady

		public ZString WaitPointReady
		{
			get
			{
				ZString result = "";
				var bookedMove = BookedMoveBO;

				if (LegBO.WaitPointDocAddressType == DocAddressType.LocalCartageCTO && Container != null && !Container.ArrivalSlotTime.IsEmpty)
				{
					result = Container.ArrivalSlotTime.ToLongTimeString();
				}
				else if (IsWaitPointTheRequestedBooking && bookedMove != null && !bookedMove.EW_RequestedDeliveryTimeStart.IsEmpty)
				{
					result = bookedMove.EW_RequestedDeliveryTimeStart.ToLongTimeString();
				}
				else if (WaitPoint != null)
				{
					result = WaitPoint.DeliverFromTime;
				}

				return result;
			}
		}

		#endregion

		#region WaitPointCloseHeading

		public ZString WaitPointCloseHeading
		{
			get
			{
				ZString result = "";

				CommonBookedCtgMove bookedMove = BookedMoveBO;
				if (IsWaitPointTheRequestedBooking && bookedMove != null && !bookedMove.EW_RequestedDeliveryTimeEnd.IsEmpty)
				{
					result = Res.GetString("2527613f-0751-4a67-895f-09ee336c5f54", "READY TO:");
				}
				else if (WaitPoint != null && !WaitPoint.DeliverToTime.IsEmpty)
				{
					result = Res.GetString("e8838002-a908-47ac-b813-005ec88b12a6", "CLOSE:");
				}

				return result;
			}
		}

		#endregion

		#region WaitPointClose

		public ZString WaitPointClose
		{
			get
			{
				ZString result = "";
				var bookedMove = BookedMoveBO;

				if (result.IsEmpty && IsWaitPointTheRequestedBooking && bookedMove != null && !bookedMove.EW_RequestedDeliveryTimeEnd.IsEmpty)
				{
					result = bookedMove.EW_RequestedDeliveryTimeEnd.ToLongTimeString();
				}
				else if (WaitPoint != null)
				{
					result = WaitPoint.DeliverToTime;
				}

				return result;
			}
		}

		#endregion

		#region WaitPointTimeIn

		public ZDateTime WaitPointTimeIn
		{
			get { return LegBO.JU_WaitPointTimeIn; }
		}

		#endregion

		#region WaitPointTimeOut

		public ZDateTime WaitPointTimeOut
		{
			get { return LegBO.JU_WaitPointTimeOut; }
		}

		#endregion

		#region WaitPointTimeDemurrage

		public ZString WaitPointTimeDemurrage
		{
			get
			{
				var waitPointDemurrage = LegBO.GetDemurrageIncludingFreeTime(LegBO.JU_CartageWaitPointDemurrage, LegBO.WaitPointDocAddress);
				return new TotalHoursHelper().GetTextFromTimeSpan(waitPointDemurrage);
			}
		}

		#endregion

		#endregion

		#region Related Objects

		#region BookedMove

		public LocalTransportBookedMoveWrapper BookedMove
		{
			get { return new LocalTransportBookedMoveWrapper(BookedMoveBO, Factory); }
		}

		#endregion

		#region Cartage

		public FreightWrapperFromCartage Cartage
		{
			get { return new FreightWrapperFromCartage(CartageBO, Factory); }
		}

		#endregion

		#region Container

		public ContainerWrapperFromCartage Container
		{
			get { return new ContainerWrapperFromCartage(new FreightWrapperFromCartage(CartageBO, Factory), ContainerBO, Factory); }
		}

		#endregion

		#region DeliverTo

		public AddressWrapper DeliverTo
		{
			get { return new AddressWrapper(LegBO.DeliverToDocAddress, Factory); }
		}

		#endregion

		#region PickupFrom

		public AddressWrapper PickupFrom
		{
			get { return new AddressWrapper(LegBO.PickupFromDocAddress, Factory); }
		}

		#endregion

		#endregion

		#region Wrapped Entities

		#region BookedMoveBO

		CommonBookedCtgMove BookedMoveBO
		{
			get { return LegBO != null ? LegBO.BookedCtgMove : null; }
		}

		#endregion

		#region CartageBO

		CommonCartage CartageBO
		{
			get { return LegBO != null ? LegBO.Cartage : null; }
		}

		#endregion

		#region ContainerBO

		CommonContainer ContainerBO
		{
			get { return LegBO != null ? LegBO.Container : null; }
		}

		#endregion

		#region LegBO

		internal CommonCartageLeg LegBO
		{
			get { return (CommonCartageLeg)WrappedBO; }
		}

		#endregion

		#endregion

		#region ParentWrapper

		internal FreightWrapper ParentWrapper { get; set; }

		#endregion
	}
}
