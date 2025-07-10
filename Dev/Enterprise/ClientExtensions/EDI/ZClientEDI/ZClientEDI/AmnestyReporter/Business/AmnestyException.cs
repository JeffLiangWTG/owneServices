using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Business;
using WTG.DevTools.Definitions;

namespace Enterprise.Client.EDI.AmnestyReporter.Business
{
	class AmnestyException
	{
		public AmnestyException(BusinessObjectFactory factory, DatAmnestyFailure datAmnestyFailure, ZGuid workItemPKHint)
		{
			this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
			this.datAmnestyFailure = datAmnestyFailure ?? throw new ArgumentNullException(nameof(datAmnestyFailure));
			wkiHint = workItemPKHint;
		}

		public ZGuid Process()
		{
			return string.IsNullOrEmpty(datAmnestyFailure.ST_Product)
				? ZGuid.Empty
				: CreateNewWorkItemOrAttachToOpenWorkItem();
		}

		ZGuid CreateNewWorkItemOrAttachToOpenWorkItem()
		{
			var workItem = factory.Load<NewWorkItem>(new ZGuid(datAmnestyFailure.EDI_IM)) ?? factory.Load<NewWorkItem>(wkiHint) ?? factory.New<NewWorkItem>();

			FillWorkItemWithValues(workItem, datAmnestyFailure);

			return workItem.PK;
		}

		static void FillWorkItemWithValues(NewWorkItem workItem, DatAmnestyFailure datAmnestyFailure)
		{
			workItem.WKI_WorkItemType = datAmnestyFailure.ST_Product;
			workItem.WKI_WorkItemArea = datAmnestyFailure.ST_ProductArea;
			workItem.WKI_ActivityType = datAmnestyFailure.ST_Module;
			workItem.WKI_ActivitySubtype = NewWorkItemLookups.WorkItemTypeConstants.AmnestyFix;
			workItem.WKI_Priority = ReleaseRings.Codes.GPR;

			workItem.WKI_Summary = "Fix Amnesty test failure";
			var description = workItem.WKI_Details.ToUTF8() +
							  string.Format(CultureInfo.InvariantCulture, "Assembly: {0}\r\nClass: {1}\r\nMethod: {2}\r\n", datAmnestyFailure.E8_AssemblyName, datAmnestyFailure.E2_TestClass, datAmnestyFailure.E6_MethodName) +
							  CrikeyWeb.GetTestFailureHistoryUrl(datAmnestyFailure.E6_PK) + "\r\n\r\n";
			workItem.WKI_Details = ZBlob.FromUTF8(description);
			workItem.Logs.AddNew(AutoEvents.AutoMatchDone,
				string.Format(CultureInfo.InvariantCulture, "Created from Crikey Monitor Amnesty Management (item {0}).", datAmnestyFailure.AF_PK));
		}

		readonly DatAmnestyFailure datAmnestyFailure;
		readonly BusinessObjectFactory factory;
		readonly ZGuid wkiHint;
	}
}
