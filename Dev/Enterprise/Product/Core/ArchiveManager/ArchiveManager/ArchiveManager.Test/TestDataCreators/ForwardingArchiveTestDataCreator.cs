using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Customs.Business;
using Enterprise.eManifest.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using ContainerPenalty = Enterprise.Freight.Business.ContainerPenalty;
using OrderLineDeliverContainer = Enterprise.Freight.Forwarding.Orders.Business.OrderLineDeliverContainer;
using OrderLineDelivery = Enterprise.Freight.Forwarding.Orders.Business.OrderLineDelivery;

namespace Enterprise.ArchiveManager.Test.TestDataCreators
{
	[CodeAlive(reason: "Used by ObjectFactory")]
	public sealed class ForwardingArchiveTestDataCreator : IForwardingTestDataCreator
	{
		public void CreateConsolData(int noOfShipmentsToCreate, int inputIndex, out ZGuid consolPK, bool isCancelled, out ZGuid[] shipmentPKs, out int outPutindex, bool isCFSLoadList = false)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var ratingTestDataCreator = ObjectFactory.Get<IRatingTestDataCreator>();

			var jobConsol = factory.New<ForwardingConsol>();
			int shipmentCounter = 0;
			for (int i = inputIndex; i < noOfShipmentsToCreate; i++)
			{
				var ratingHeaderPK = ratingTestDataCreator.CreateAttachedRatingData(i.ToString(), isCancelled);
				jobConsol.Shipments.Add(CreateShipmentData(factory, ratingHeaderPK, i, isCancelled, isCFSLoadList));
				shipmentCounter = i;
			}
			outPutindex = shipmentCounter;
			if (isCancelled)
			{
				jobConsol.IsCancelled = true;
			}
			factory.Save();

			ConsolExportAWBHeader consolExportAWBHeader = factory.New<ConsolExportAWBHeader>();
			consolExportAWBHeader.EH_ParentID = jobConsol.PK;
			consolExportAWBHeader.EH_Table = "JobConsol";

			var jobCartage = (BusinessObject)factory.New<ICommonCartage>();
			jobCartage[JobCartageSchema.JJ_ParentID] = jobConsol.PK;
			jobCartage[JobCartageSchema.JJ_ParentTableCode] = JobConsolSchema.Constants.Prefix;
			jobCartage[JobCartageSchema.JJ_ConsignmentID] = jobConsol.JK_UniqueConsignRef + "/I";

			if (isCancelled)
			{
				jobCartage[JobCartageSchema.JJ_IsCancelled] = true;
			}

			if (outPutindex < 3)
			{
				var jobContainerPenalty = factory.NewWithValidTestData<ContainerPenalty>();
				jobContainerPenalty.Container.JC_JK = jobConsol.PK;
				jobContainerPenalty.Container.JC_ContainerJobID = string.Empty;
			}

			jobConsol.JK_IsCFS = isCFSLoadList;

			factory.Save();

			consolPK = jobConsol.PK;
			shipmentPKs = jobConsol.Shipments.GetPKs().ToArray();
		}

		CommonShipment CreateShipmentData(BusinessObjectFactory factory, ZGuid ratingHeaderPK, int deciderOfShipmentsWithOrWithoutContainer, bool isCancelled, bool isCFSShipment = false)
		{
			ZGuid testOrgHeaderPK = new ZGuid("0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1");
			var orgHeader = factory.Load<OrgHeader>(testOrgHeaderPK);

			CommonShipment jobShipment = factory.New<ForwardingShipment>();
			jobShipment.JS_TH_OneTimeQuote = ratingHeaderPK;
			if (isCancelled)
			{
				jobShipment.IsCancelled = true;
			}
			jobShipment.JS_IsCFSRegistered = isCFSShipment;

			factory.Save();
			JobShipmentPreplanning jobShipmentPreplanning = factory.New<JobShipmentPreplanning>();
			jobShipmentPreplanning.BuyerPK = testOrgHeaderPK;
			jobShipmentPreplanning.EF_JS = jobShipment.PK;

			var jobShipmentGateway = factory.New<ShipmentGateway>();
			jobShipmentGateway.JSG_JS_Shipment = jobShipment.PK;
			jobShipmentGateway.JSG_OA_ForwarderAddress = orgHeader.MainAddress.PK;

			if (deciderOfShipmentsWithOrWithoutContainer < 3)
			{
				CommonContainer jobContainer = factory.New<CommonContainer>();
				jobContainer.JC_JS_FCLBookingOnlyLink = jobShipment.PK;

				var jobContainerPenalty = factory.NewWithValidTestData<ContainerPenalty>();
				jobContainer.ImportPenalties.Add(jobContainerPenalty);

				PackLine packLine = jobShipment.OuterPackLines.AddNew();
				packLine.JL_JS = jobShipment.PK;

				PackLocation jobPackLoc = factory.New<PackLocation>();
				jobPackLoc.JQ_JL = packLine.PK;

				JobContainerPackPivot jobContainerPackPivot = factory.New<JobContainerPackPivot>();
				jobContainerPackPivot.J6_JC = jobContainer.PK;
				jobContainerPackPivot.J6_JL = packLine.PK;

				OrderLineDelivery jobOrderLineDelivery = factory.New<OrderLineDelivery>();

				OrderLineDeliverContainer jobOrderLineDeliverContainer = factory.New<OrderLineDeliverContainer>();
				jobOrderLineDeliverContainer.J5_JC = jobContainer.PK;
				jobOrderLineDeliverContainer.J5_J4 = jobOrderLineDelivery.PK;

				var jobCartage = (BusinessObject)factory.New<ICommonCartage>();
				jobCartage[JobCartageSchema.JJ_ParentID] = jobShipment.PK;
				jobCartage[JobCartageSchema.JJ_ParentTableCode] = JobShipmentSchema.Constants.Prefix;
				jobCartage[JobCartageSchema.JJ_ConsignmentID] = jobShipment.JS_UniqueConsignRef + "/I";

				var bookedCartageContainerMove = (BusinessObject)factory.New<ICommonBookedCtgMove>();
				bookedCartageContainerMove[JobBookedCtgMoveSchema.EW_JC_Container] = jobContainer.PK;
				bookedCartageContainerMove[JobBookedCtgMoveSchema.EW_JJ] = jobCartage.PK;

				//Splits
				CommonShipment splitJobShipment = factory.New<ForwardingShipment>();
				splitJobShipment.JS_JS_SplitSwitchShipment = jobShipment.PK;

				CommonContainer splitJobContainer = factory.New<CommonContainer>();
				splitJobContainer.JC_JS_FCLBookingOnlyLink = splitJobShipment.PK;

				PackLine splitPackLine = factory.New<PackLine>();
				splitPackLine.JL_JS = splitJobShipment.PK;

				JobContainerPackPivot splitJobContainerPackPivot = factory.New<JobContainerPackPivot>();
				splitJobContainerPackPivot.J6_JC = splitJobContainer.PK;
				splitJobContainerPackPivot.J6_JL = splitPackLine.PK;

				//Coload
				CommonShipment coLoadJobShipment = factory.New<ForwardingShipment>();
				coLoadJobShipment.JS_JS_SplitSwitchShipment = jobShipment.PK;

				CommonContainer coLoadJobContainer = factory.New<CommonContainer>();
				coLoadJobContainer.JC_JS_FCLBookingOnlyLink = coLoadJobShipment.PK;

				var coLoadPackLine = factory.New<PackLine>();
				coLoadPackLine.JL_JS = coLoadJobShipment.PK;

				var coLoadJobContainerPackPivot = factory.New<JobContainerPackPivot>();
				coLoadJobContainerPackPivot.J6_JC = coLoadJobContainer.PK;
				coLoadJobContainerPackPivot.J6_JL = coLoadPackLine.PK;

				var jobPackLineHarmonisedCode = factory.New<JobPackLineHarmonisedCode>();
				jobPackLineHarmonisedCode.JLH_JL = coLoadPackLine.PK;
				jobPackLineHarmonisedCode.JLH_Code = "HS1";
				jobPackLineHarmonisedCode.JLH_RN_NKCountry = "AU";

				var packageParent = factory.New<DummyBusinessObject>();

				var packageJob = factory.New<PkgPackageJob>();
				packageJob.KJ_ParentID = packageParent.PK;
				packageJob.KJ_ParentTableCode = packageParent.TablePrefix;

				var pkg = factory.New<PkgPackage>();
				pkg.KP_KJ_ParentPackageJob = packageJob.PK;
				pkg.KP_Volume = 10m;
				pkg.KP_Sequence = 1;
				pkg.KP_F3_NKPackType = "PKG";

				var jobPackLinePackage = factory.New<JobPackLinePackage>();
				jobPackLinePackage.JPP_JL_PackLine = coLoadPackLine.PK;
				jobPackLinePackage.JPP_KP_Packge = pkg.PK;
			}

			ShipmentExportAWBHeader shipmentExportAWBHeader = factory.New<ShipmentExportAWBHeader>();
			shipmentExportAWBHeader.EH_ParentID = jobShipment.PK;
			shipmentExportAWBHeader.EH_Table = "JobShipment";  //check if table or code

			OrderItem orderItem = jobShipment.DocsAndCartage.OrderItems.AddNew();
			orderItem.JT_OrderReference = jobShipment.JS_UniqueConsignRef;

			factory.Save();
			return jobShipment;
		}

		public void CreateCartageData(ZGuid pk, out ZGuid cartagePK, bool isCancelled = false)
		{
			var factory = new BusinessObjectFactory();

			var jobCartage = (BusinessObject)factory.New<ICommonCartage>();
			jobCartage[JobCartageSchema.JJ_ParentID] = pk;
			jobCartage[JobCartageSchema.JJ_ParentTableCode] = JobShipmentSchema.Constants.Prefix;
			jobCartage[JobCartageSchema.JJ_ConsignmentID] = "T00001000/I";
			jobCartage[JobCartageSchema.JJ_IsCancelled] = isCancelled;

			factory.Save();

			cartagePK = jobCartage.PK;
		}

		public void CreateCartageDataSafeDuplicate(ZGuid pk, out ZGuid cartagePK, bool isCancelled = false)
		{
			var factory = new BusinessObjectFactory();

			var jobCartage = (BusinessObject)factory.New<ICommonCartage>();
			jobCartage[JobCartageSchema.JJ_ParentID] = pk;
			jobCartage[JobCartageSchema.JJ_ParentTableCode] = JobShipmentSchema.Constants.Prefix;
			jobCartage[JobCartageSchema.JJ_IsCancelled] = isCancelled;

			factory.Save();

			cartagePK = jobCartage.PK;
		}

		void IForwardingTestDataCreator.CreateShipmentData(out ZGuid shipmentPK)
		{
			var factory = new BusinessObjectFactory();
			var jobShipment = factory.New<ForwardingShipment>();
			factory.Save();
			shipmentPK = jobShipment.PK;
		}

		void IForwardingTestDataCreator.CreateShipmentData(out ZGuid shipmentPK, bool isCancelled)
		{
			var factory = new BusinessObjectFactory();
			var jobShipment = factory.New<ForwardingShipment>();
			jobShipment.JS_IsCancelled = isCancelled;
			factory.Save();
			shipmentPK = jobShipment.PK;
		}

		void IForwardingTestDataCreator.CreateConsolDataWithShipment(out ZGuid consolPK, out ZGuid shipmentPK, bool isCancelled)
		{
			var factory = new BusinessObjectFactory();

			var consol = factory.New<ForwardingConsol>();
			consol.JK_IsCancelled = true;
			var shipment = consol.Shipments.AddNew();

			factory.Save();

			shipmentPK = shipment.PK;
			consolPK = consol.PK;
		}

		public ZGuid CreateShipmentWithMessagesData()
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.New<CommonShipment>();

			var message = shipment.Messages.AddNew();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.GenericMessageDelivery;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Shipments;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_ApplicationReference = "REF";
			message.EM_MessageNum = "123";

			var interchange = factory.NewWithValidTestData<EDIInterchange>();
			message.EM_EI = interchange.PK;

			factory.Save();

			return shipment.PK;
		}

		public List<BusinessObject> CreateArchiveableForwardingData(BusinessObjectFactory factory, bool isActiveProcess = true)
		{
			var archiveableObjects = new List<BusinessObject>();

			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001";
			if (!isActiveProcess)
			{
				shipment.JS_IsCancelled = true;
			}
			archiveableObjects.Add(shipment);

			var consol = factory.NewWithValidTestData<ForwardingConsol>();
			if (!isActiveProcess)
			{
				consol.JK_IsCancelled = true;
			}
			archiveableObjects.Add(consol);

			var shipmentHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			shipmentHeader.JH_Status = JobHeaderStatus.Closed.Code;
			shipmentHeader.JH_A_JCL = ZDateTime.BrettsBirthday;
			shipmentHeader.JH_JobNum = "T0001";
			shipmentHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			shipmentHeader.JH_ParentID = shipment.PK;
			archiveableObjects.Add(shipmentHeader);

			var consolHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			consolHeader.JH_Status = JobHeaderStatus.Closed.Code;
			consolHeader.JH_A_JCL = ZDateTime.BrettsBirthday;
			consolHeader.JH_JobNum = "T00002";
			consolHeader.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
			consolHeader.JH_ParentID = consol.PK;
			archiveableObjects.Add(consolHeader);

			factory.Save();

			archiveableObjects.AddRange(CreateArchiveableForwardingData_ChildrenOfJobShipment(factory, shipment));
			archiveableObjects.AddRange(CreateArchiveableForwardingData_ChildrenOfJobConsol(factory, consol));
			archiveableObjects.AddRange(CreateArchiveableForwardingData_childrenOfJobHeader(factory, consolHeader));

			return archiveableObjects;
		}

		List<BusinessObject> CreateArchiveableForwardingData_ChildrenOfJobShipment(BusinessObjectFactory factory, ForwardingShipment shipment)
		{
			var archiveableObjects = new List<BusinessObject>();

			var supplierBookingline = factory.NewWithValidTestData<SupplierBookingLine>();
			supplierBookingline.DL_JS_ApprovedShipment = shipment.PK;
			archiveableObjects.Add(supplierBookingline);

			var shipmentProfitShares = factory.NewWithValidTestData<ShipmentProfitShares>();
			shipmentProfitShares.PSS_JS = shipment.PK;
			archiveableObjects.Add(shipmentProfitShares);

			factory.Save();

			return archiveableObjects;
		}

		List<BusinessObject> CreateArchiveableForwardingData_ChildrenOfJobConsol(BusinessObjectFactory factory, ForwardingConsol consol)
		{
			var archiveableObjects = new List<BusinessObject>();

			var childConsol = factory.NewWithValidTestData<ForwardingConsol>();
			childConsol.JK_JK_MasterConsol = consol.PK;
			archiveableObjects.Add(childConsol);

			var jobConShipLink = factory.NewWithValidTestData<JobConShipLink>();
			jobConShipLink.JN_JK = consol.PK;
			archiveableObjects.Add(jobConShipLink);

			var jobConsolDgRestrictions = factory.NewWithValidTestData<ConsolDGRestrictions>();
			jobConsolDgRestrictions.JKD_JK = consol.PK;
			archiveableObjects.Add(jobConsolDgRestrictions);

			var hvlvOuterPackage = factory.NewWithValidTestData<HVLVOuterPackage>();
			hvlvOuterPackage.HVO_JK_LoadedOnConsol = consol.PK;
			archiveableObjects.Add(hvlvOuterPackage);

			var cusMAWB = factory.NewWithValidTestData<CusMAWB>();
			cusMAWB.CM_JK = consol.PK;
			archiveableObjects.Add(cusMAWB);

			var jobContainer = factory.NewWithValidTestData<ForwardingContainer>();
			jobContainer.JC_JK = consol.PK;
			archiveableObjects.Add(jobContainer);

			var jobConsolAWBSpecialHandling = consol.AWBSpecialHandlingItems.AddNew();
			jobConsolAWBSpecialHandling.JKH_Code = "EAW";
			archiveableObjects.Add(jobConsolAWBSpecialHandling);

			var consolidationProfitShare = factory.NewWithValidTestData<ConsolidationProfitShare>();
			consolidationProfitShare.CPS_JK = consol.PK;
			archiveableObjects.Add(consolidationProfitShare);

			var profitShareRedistribution = factory.NewWithValidTestData<ProfitShareRedistribution>();
			consolidationProfitShare.CPS_PSR = profitShareRedistribution.PK;
			archiveableObjects.Add(profitShareRedistribution);

			var shipmentProfitShares = factory.NewWithValidTestData<ShipmentProfitShares>();
			shipmentProfitShares.PSS_CPS = consolidationProfitShare.PK;
			archiveableObjects.Add(shipmentProfitShares);

			factory.Save();

			return archiveableObjects;
		}

		public List<BusinessObject> CreateArchiveableForwardingData_childrenOfJobHeader(BusinessObjectFactory factory, JobHeader jobHeader)
		{
			var archiveableObjects = new List<BusinessObject>();

			var consolidationProfitShare = factory.NewWithValidTestData<ConsolidationProfitShare>();
			consolidationProfitShare.CPS_JH_ConsolJob = jobHeader.PK;
			archiveableObjects.Add(consolidationProfitShare);

			var shipmentProfitShares = factory.NewWithValidTestData<ShipmentProfitShares>();
			shipmentProfitShares.PSS_JH_ShipmentJob = jobHeader.PK;
			archiveableObjects.Add(shipmentProfitShares);

			factory.Save();

			return archiveableObjects;
		}
	}
}
