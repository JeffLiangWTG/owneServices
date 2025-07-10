using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GenericConsol
{
	[TestedAsNonPersistentBusinessObject]
	[CodeProperty(AutoViewGenericConsol.Schema.VX_Code)]
	[DescriptionProperty(AutoViewGenericConsol.Schema.VX_Description)]
	public class GenericConsol : AutoViewGenericConsol
	{
		public GenericConsol(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Business Object Overrides

		public override void Delete()
		{
			// Don't do anything since GenericConsol is nonpersistent
		}

		#endregion

		public DataContextType GetDataContextType
		{
			get
			{
				var context = GenericConsolHelper.GetDataContextTypeByParentTableCode(VX_ParentTableCode);
				return context != null
					? (DataContextType)context
					: throw new NotSupportedException("No data context has been implemented with that code.");
			}
		}

		public ZDateTime VX_ETD
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				IJobCostingPlugIn plugIn = LoadConsolBOFromGenericConsol(Factory, this);
				if (plugIn != null && plugIn.CostSupporter != null)
				{
					result = plugIn.CostSupporter.ETD;
				}
				return result;
			}
		}

		public ZPropertyInfo VX_ETDInfo
		{
			get { return GetZPropertyInfo(nameof(VX_ETD)); }
		}

		public ZDateTime VX_ETA
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				IJobCostingPlugIn plugIn = LoadConsolBOFromGenericConsol(Factory, this);
				if (plugIn != null && plugIn.CostSupporter != null)
				{
					result = plugIn.CostSupporter.ETA;
				}
				return result;
			}
		}

		public ZPropertyInfo VX_ETAInfo
		{
			get { return GetZPropertyInfo(nameof(VX_ETA)); }
		}

		#region Controller ID

		public ControllerID GetParentConsolController()
		{
			ControllerID controller = null;
			switch (VX_ParentTableCode)
			{
				case JobConsolSchema.Constants.Prefix:
					controller = ControllerIDs.JobConsol;
					break;

				case DtbBookingConsolidationSchema.Constants.Prefix:
					controller = ControllerIDs.DtbBookingConsolidation;
					break;

				case DtbBookingSchema.Constants.Prefix:
					controller = ControllerIDs.DtbBooking;
					break;

				case DtbConsignmentRunSheetSchema.Constants.Prefix:
					controller = ControllerIDs.DtbConsignmentRunSheet;
					break;

				case JobCartageRunSheetSchema.Constants.Prefix:
					controller = ControllerIDs.CartageWorkSheet;
					break;

				case WhsItemReceiveTransportationUnitSchema.Constants.Prefix:
					controller = ControllerIDs.WhsItemReceiveTransportationUnit;
					break;

				case WhsItemDispatchLoadListSchema.Constants.Prefix:
					controller = ControllerIDs.WhsItemDispatchLoadList;
					break;

				case WhsItemDispatchTransportationUnitSchema.Constants.Prefix:
					controller = ControllerIDs.WhsItemDispatchTransportationUnit;
					break;

				default:
					controller = ControllerIDs.JobConsol;
					break;
			}
			return controller;
		}

		#endregion

		public static IJobCostingPlugIn LoadConsolBOFromParentIdAndCode(BusinessObjectFactory factory, ZGuid parentID, ZString parentTableCode)
		{
			IJobCostingPlugIn result = null;

			switch (parentTableCode.Trim())
			{
				case "":
					result = GetIJobCostingPlugInByPK(factory, parentID);
					break;

				case JobConsolSchema.Constants.Prefix:
					result = factory.Load<ForwardingConsol>(parentID);
					break;

				case DtbBookingConsolidationSchema.Constants.Prefix:
				case DtbBookingSchema.Constants.Prefix:
				case DtbConsignmentRunSheetSchema.Constants.Prefix:
				case DtbLinehaulManifestSchema.Constants.Prefix:
				case JobCartageRunSheetSchema.Constants.Prefix:
				case WhsItemReceiveTransportationUnitSchema.Constants.Prefix:
				case WhsItemDispatchLoadListSchema.Constants.Prefix:
				case WhsItemDispatchTransportationUnitSchema.Constants.Prefix:
					result = factory.Load<IJobCostingPlugIn>(parentTableCode, parentID);
					break;

				default:
					throw new ApplicationException("Fail to find IJobCostingPlugIn due to VX_ParentTableCode is missing or invalid");
			}

			return result;
		}

		public static IJobCostingPlugIn LoadConsolBOFromGenericConsol(BusinessObjectFactory factory, GenericConsol genericConsol)
		{
			if (genericConsol != null)
			{
				switch (genericConsol.VX_ParentTableCode)
				{
					case JobConsolSchema.Constants.Prefix:
						return factory.Load<ForwardingConsol>(genericConsol.PK);

					case DtbBookingConsolidationSchema.Constants.Prefix:
					case DtbBookingSchema.Constants.Prefix:
					case DtbConsignmentRunSheetSchema.Constants.Prefix:
					case DtbLinehaulManifestSchema.Constants.Prefix:
					case JobCartageRunSheetSchema.Constants.Prefix:
					case WhsItemReceiveTransportationUnitSchema.Constants.Prefix:
					case WhsItemDispatchLoadListSchema.Constants.Prefix:
					case WhsItemDispatchTransportationUnitSchema.Constants.Prefix:
						return factory.Load<IJobCostingPlugIn>(genericConsol.VX_ParentTableCode, genericConsol.PK);

					default:
						throw new ApplicationException("Fail to find IJobCostingPlugIn due to VX_ParentTableCode is missing or invalid");
				}
			}
			return null;
		}

		public static IJobCostingPlugIn GetIJobCostingPlugInByPrimaryCode(BusinessObjectFactory factory, ZString primaryNumber, ZString consolType)
		{
			var query = new ZQuery(ViewGenericConsolSchema.VX_Code, primaryNumber);
			if (!consolType.IsEmpty)
			{
				query.AddToFilter(ViewGenericConsolSchema.VX_ParentTableCode, consolType);
			}
			GenericConsol genericConsol = factory.LoadTop1<GenericConsol>(query);
			return LoadConsolBOFromGenericConsol(factory, genericConsol);
		}

		public static IJobCostingPlugIn GetIJobCostingPlugInByPrimaryCode(BusinessObjectFactory factory, ZString primaryNumber)
		{
			return GetIJobCostingPlugInByPrimaryCode(factory, primaryNumber, string.Empty);
		}

		public static ZGuid GetConsolPKByPrimaryCode(BusinessObjectFactory factory, ZString primaryNumber, ZString consolType)
		{
			var query = new ZQuery(ViewGenericConsolSchema.VX_Code, primaryNumber);
			if (!consolType.IsEmpty)
			{
				query.AddToFilter(ViewGenericConsolSchema.VX_ParentTableCode, consolType);
			}
			var genericConsol = factory.LoadTop1<GenericConsol>(query);
			return genericConsol != null ? genericConsol.PK : ZGuid.Empty;
		}

		public static ZGuid GetConsolPKByPrimaryCode(BusinessObjectFactory factory, ZString primaryNumber)
		{
			return GetConsolPKByPrimaryCode(factory, primaryNumber, string.Empty);
		}

		public static IJobCostingPlugIn GetIJobCostingPlugInBySecondaryCode(BusinessObjectFactory factory, ZString secondaryNumber, ZString consolType)
		{
			var query = new ZQuery(ViewGenericConsolSchema.VX_SecondaryCode, secondaryNumber);
			if (!consolType.IsEmpty)
			{
				query.AddToFilter(ViewGenericConsolSchema.VX_ParentTableCode, consolType);
			}
			GenericConsol genericConsol = factory.LoadTop1<GenericConsol>(query);
			return LoadConsolBOFromGenericConsol(factory, genericConsol);
		}

		public static IJobCostingPlugIn GetIJobCostingPlugInBySecondaryCode(BusinessObjectFactory factory, ZString secondaryNumber)
		{
			return GetIJobCostingPlugInBySecondaryCode(factory, secondaryNumber, string.Empty);
		}

		public static ZString GetParentTableCodeFromParentId(BusinessObjectFactory factory, ZGuid parentID)
		{
			GenericConsol genericConsol = factory.Load<GenericConsol>(parentID);
			return genericConsol != null ? genericConsol.VX_ParentTableCode : ZString.Empty;
		}

		public static int GetCountofIJobCostingPlugInByPrimaryCode(BusinessObjectFactory factory, ZString primaryNumber, ZString consolType)
		{
			var query = new ZQuery(ViewGenericConsolSchema.VX_Code, primaryNumber);
			if (!consolType.IsEmpty)
			{
				query.AddToFilter(ViewGenericConsolSchema.VX_ParentTableCode, consolType);
			}
			GenericConsol[] genericConsols = factory.Load<GenericConsol>(query);
			return genericConsols.Length;
		}

		public static int GetCountofIJobCostingPlugInByPrimaryCode(BusinessObjectFactory factory, ZString primaryNumber)
		{
			return GetCountofIJobCostingPlugInByPrimaryCode(factory, primaryNumber, string.Empty);
		}

		public static int GetCountofIJobCostingPlugInBySecondaryCode(BusinessObjectFactory factory, ZString secondaryNumber, ZString consolType)
		{
			var query = new ZQuery(ViewGenericConsolSchema.VX_SecondaryCode, secondaryNumber);
			if (!consolType.IsEmpty)
			{
				query.AddToFilter(ViewGenericConsolSchema.VX_ParentTableCode, consolType);
			}
			GenericConsol[] genericConsols = factory.Load<GenericConsol>(query);
			return genericConsols.Length;
		}

		public static int GetCountofIJobCostingPlugInBySecondaryCode(BusinessObjectFactory factory, ZString secondaryNumber)
		{
			return GetCountofIJobCostingPlugInBySecondaryCode(factory, secondaryNumber, string.Empty);
		}

		public static IJobCostingPlugIn GetIJobCostingPlugInByPK(BusinessObjectFactory factory, ZGuid consolPK, ZString consolType)
		{
			ZQuery query = new ZQuery(ViewGenericConsolSchema.PK, consolPK);
			if (!consolType.IsEmpty)
			{
				query.AddToFilter(ViewGenericConsolSchema.VX_ParentTableCode, consolType);
			}
			GenericConsol genericConsol = factory.LoadTop1<GenericConsol>(query);
			return LoadConsolBOFromGenericConsol(factory, genericConsol);
		}

		public static IJobCostingPlugIn GetIJobCostingPlugInByPK(BusinessObjectFactory factory, ZGuid consolPK)
		{
			return GetIJobCostingPlugInByPK(factory, consolPK, string.Empty);
		}
	}
}
