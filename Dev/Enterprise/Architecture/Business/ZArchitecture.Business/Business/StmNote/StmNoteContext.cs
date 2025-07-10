using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public class StmNoteContexts
	{
		public static StmNoteContexts Default
		{
			get
			{
				StmNoteContexts result = new StmNoteContexts();
				result.Module = StmNoteContextModule.A;
				result.Direction = StmNoteContextDirection.A;
				result.FreightMode = StmNoteContextFreightMode.A;
				return result;
			}
		}

		public StmNoteContextModule Module;
		public StmNoteContextDirection Direction;
		public StmNoteContextFreightMode FreightMode;

		public override string ToString()
		{
			return (Module == StmNoteContextModule.Undefined ? "_" : Module.ToString()) + // String is empty or contains only symbols.
				   (Direction == StmNoteContextDirection.Undefined ? "_" : Direction.ToString()) +
				   (FreightMode == StmNoteContextFreightMode.Undefined ? "_" : FreightMode.ToString());
		}
	}

	#region StmNoteContexts codes

	[Flags]
	public enum StmNoteContextModule
	{
		Undefined = 0,
		A = 1 << 0,     //All
		F = 1 << 1,     //Forwarding (shipment+consol)
		C = 1 << 3,     //CFS
		D = 1 << 4,     //Customs/Declarations
		E = 1 << 5,     //Shipment and Declaration
		O = 1 << 6,     //Orders
		I = 1 << 7,     //Forwarding, Brokerage, CFS and Orders
		T = 1 << 8,     //Transport
		W = 1 << 9,     //Warehouse
		S = 1 << 10     //Ships Agency
	}

	[Flags]
	public enum StmNoteContextDirection
	{
		Undefined = 0,
		A = 1 << 0,     //All
		I = 1 << 1,     //Import						//also (warehouse) Internal
		E = 1 << 2,     //Export
		B = 1 << 3,     //Import and Export
		D = 1 << 4,     //Domestic
		X = 1 << 5,     //Cross Trade (?)
		F = 1 << 6,     //All Forwarding (?)
		O = 1 << 7,     //Other (anything but above?)	//also (warehouse) Out
						//next is for warehouse module filters only
		R = 1 << 8      //(warehouse) In
	}

	[Flags]
	public enum StmNoteContextFreightMode
	{
		Undefined = 0,
		A = 1 << 0,     //All
		S = 1 << 1,     //Sea									//also (warehouse) Stocktake / Cyclic Count
		F = 1 << 2,     //FCL (full container load)
		L = 1 << 3,     //LCL (less container load)
		I = 1 << 4,     //Air
		R = 1 << 5,     //Road									//also (warehouse) Receive // also (warehouse) Release
		W = 1 << 6,     //Rail									//also (warehouse) Work Order
		B = 1 << 7,     //Air and Sea
						//next is for warehouse module filters only
		O = 1 << 8,     //(warehouse) Orders
		T = 1 << 9,     //(warehouse) Transfers
		D = 1 << 10,    //(warehouse) Adjustments
		P = 1 << 11     //(warehouse) Periodic Billing
	}

	#region Warehouse-related StmNoteContexts parts

	[WTG.StaticAnalysis.Annotation.CodeAlive("This is consumed in StmNote using nameof()")]
	[Flags]
	public enum StmNoteContextWarehouseDirection
	{
		Undefined = 0,
		A = 1 << 0,     //All
		R = 1 << 1,     //In
		O = 1 << 2,     //Out
		I = 1 << 3      //Internal
	}

	#endregion

	internal static class StmNoteCaptions
	{
		internal static Dictionary<ZString, MultilingualString> Module
		{
			get
			{
				if (module == null)
				{
					module = new Dictionary<ZString, MultilingualString>();
					module.Add("A", ResString.GetMultilingualString("2b5bc3d0-15c4-46e6-8c67-7d2c5027747b", "A - All"));
					module.Add("F", ResString.GetMultilingualString("e816bb92-76c4-40c9-a809-537c497b7066", "F - Forwarding"));
					module.Add("C", ResString.GetMultilingualString("ff26b9fc-f6b6-4433-a190-801de35c27eb", "C - CFS"));
					module.Add("D", ResString.GetMultilingualString("d16334a8-c1ff-4c5b-a748-84147c4a7b01", "D - Customs/Declarations"));
					module.Add("E", ResString.GetMultilingualString("760c9719-d942-4520-96bb-0ea69cdb3f53", "E - Shipment and Declaration"));
					module.Add("O", ResString.GetMultilingualString("6aec6eb3-5325-49ea-8c78-66dc9e1ef0e1", "O - Orders"));
					module.Add("I", ResString.GetMultilingualString("efa2a363-f06a-4b2a-9ce1-0695f872fb59", "I - Forwarding, Brokerage, CFS and Orders"));
					module.Add("T", ResString.GetMultilingualString("cc1b93ba-e986-4ad5-94ba-f295aac7b791", "T - Transport"));
					module.Add("W", ResString.GetMultilingualString("fbc47c8e-48bf-43c5-8be0-42c2ad68168e", "W - Warehouse"));
					module.Add("S", ResString.GetMultilingualString("45a88101-803a-4470-a6ce-82f70df41bbf", "S - Ships Agency"));
				}
				return module;
			}
		}
		[ThreadStatic]
		static Dictionary<ZString, MultilingualString> module;

		internal static Dictionary<ZString, MultilingualString> Direction
		{
			get
			{
				if (direction == null)
				{
					direction = new Dictionary<ZString, MultilingualString>();
					direction.Add("A", ResString.GetMultilingualString("2b5bc3d0-15c4-46e6-8c67-7d2c5027747b", "A - All"));
					direction.Add("I", ResString.GetMultilingualString("47f2fc96-9c5d-48d0-be3d-d4d823606fed", "I - Import"));
					direction.Add("E", ResString.GetMultilingualString("b6caafa3-ea55-4140-9a15-6d0368b2455e", "E - Export"));
					direction.Add("B", ResString.GetMultilingualString("30ab6315-9b2b-4331-8880-32a604599200", "B - Import and Export"));
					direction.Add("D", ResString.GetMultilingualString("62bc2021-9461-412e-ae0f-4fdb585a31da", "D - Domestic"));
					direction.Add("X", ResString.GetMultilingualString("e041034d-be5e-488d-aa1d-d2a6312b80c2", "X - Cross Trade"));
					direction.Add("F", ResString.GetMultilingualString("ef6ed1d3-4d9c-4a52-9c04-39e3addecfff", "F - All Forwarding"));
					direction.Add("O", ResString.GetMultilingualString("fb79c793-f144-4889-a805-8c3a228b57e8", "O - Other"));
				}
				return direction;
			}
		}
		[ThreadStatic]
		static Dictionary<ZString, MultilingualString> direction;

		internal static Dictionary<ZString, MultilingualString> FreightMode
		{
			get
			{
				if (freightMode == null)
				{
					freightMode = new Dictionary<ZString, MultilingualString>();
					freightMode.Add("A", ResString.GetMultilingualString("2b5bc3d0-15c4-46e6-8c67-7d2c5027747b", "A - All"));
					freightMode.Add("S", ResString.GetMultilingualString("2b6bfbc3-d5e0-4fd3-ba0c-f7b8a7f700ed", "S - Sea"));
					freightMode.Add("F", ResString.GetMultilingualString("9d57860b-c91f-4ecd-ac7a-caff53cc15cb", "F - FCL"));
					freightMode.Add("L", ResString.GetMultilingualString("ac501764-0b60-45a0-8926-8aa29196e415", "L - LCL"));
					freightMode.Add("I", ResString.GetMultilingualString("3b45fcc0-1e6a-4147-af29-ffba8beede80", "I - Air"));
					freightMode.Add("R", ResString.GetMultilingualString("407f198c-a5ed-4dc6-8782-82db029b4996", "R - Road"));
					freightMode.Add("W", ResString.GetMultilingualString("1c2a38e1-531d-42f6-9c1c-799fcc4b913f", "W - Rail"));
					freightMode.Add("B", ResString.GetMultilingualString("bfd23d2c-35db-46ad-a098-5693813291ad", "B - Air and Sea"));
				}
				return freightMode;
			}
		}
		[ThreadStatic]
		static Dictionary<ZString, MultilingualString> freightMode;

		#region Warehouse-related captions

		internal static Dictionary<ZString, MultilingualString> DirectionWarehouse
		{
			get
			{
				if (directionWarehouse == null)
				{
					directionWarehouse = new Dictionary<ZString, MultilingualString>();
					directionWarehouse.Add("A", ResString.GetMultilingualString("2b5bc3d0-15c4-46e6-8c67-7d2c5027747b", "A - All"));
					directionWarehouse.Add("R", ResString.GetMultilingualString("2175426d-0717-4853-93f6-88414f3c3d8c", "R - In"));
					directionWarehouse.Add("O", ResString.GetMultilingualString("afb2c2d0-5140-42e0-9f26-fc876707e03c", "O - Out"));
					directionWarehouse.Add("I", ResString.GetMultilingualString("2b549e05-67bc-4de4-aeeb-11e03dbf2fea", "I - Internal"));
				}
				return directionWarehouse;
			}
		}
		[ThreadStatic]
		static Dictionary<ZString, MultilingualString> directionWarehouse;

		internal static Dictionary<ZString, MultilingualString> FreightModeWarehouseR
		{
			get
			{
				if (freightModeWarehouseR == null)
				{
					freightModeWarehouseR = new Dictionary<ZString, MultilingualString>();
					freightModeWarehouseR.Add("A", ResString.GetMultilingualString("2b5bc3d0-15c4-46e6-8c67-7d2c5027747b", "A - All"));
					freightModeWarehouseR.Add("R", ResString.GetMultilingualString("0d956e0e-4871-42b3-bdeb-15b79e2a5a4b", "R - Receive"));
				}
				return freightModeWarehouseR;
			}
		}
		[ThreadStatic]
		static Dictionary<ZString, MultilingualString> freightModeWarehouseR;

		internal static Dictionary<ZString, MultilingualString> FreightModeWarehouseO
		{
			get
			{
				if (freightModeWarehouseO == null)
				{
					freightModeWarehouseO = new Dictionary<ZString, MultilingualString>();
					freightModeWarehouseO.Add("A", ResString.GetMultilingualString("2b5bc3d0-15c4-46e6-8c67-7d2c5027747b", "A - All"));
					freightModeWarehouseO.Add("O", ResString.GetMultilingualString("6aec6eb3-5325-49ea-8c78-66dc9e1ef0e1", "O - Orders"));
					freightModeWarehouseO.Add("R", ResString.GetMultilingualString("40a462c5-c212-43cc-9bda-19f3719612cd", "R - Release"));
					freightModeWarehouseO.Add("T", ResString.GetMultilingualString("258d2150-d2d3-479d-829e-c1bb3d157e57", "T - Transfers"));
				}
				return freightModeWarehouseO;
			}
		}
		[ThreadStatic]
		static Dictionary<ZString, MultilingualString> freightModeWarehouseO;

		internal static Dictionary<ZString, MultilingualString> FreightModeWarehouseI
		{
			get
			{
				if (freightModeWarehouseI == null)
				{
					freightModeWarehouseI = new Dictionary<ZString, MultilingualString>();
					freightModeWarehouseI.Add("A", ResString.GetMultilingualString("2b5bc3d0-15c4-46e6-8c67-7d2c5027747b", "A - All"));
					freightModeWarehouseI.Add("T", ResString.GetMultilingualString("258d2150-d2d3-479d-829e-c1bb3d157e57", "T - Transfers"));
					freightModeWarehouseI.Add("D", ResString.GetMultilingualString("8eab5f04-82ad-4523-995e-9bfcd32973d3", "D - Adjustments"));
					freightModeWarehouseI.Add("S", ResString.GetMultilingualString("f27ef54b-f907-4c76-9839-bf131ea41310", "S - Stocktake / Cyclic Count"));
					freightModeWarehouseI.Add("P", ResString.GetMultilingualString("eac6362a-3a92-4d17-b5ce-7eb4cbbc945f", "P - Periodic Billing"));
				}
				return freightModeWarehouseI;
			}
		}
		[ThreadStatic]
		static Dictionary<ZString, MultilingualString> freightModeWarehouseI;

		#endregion
	}

	#endregion

	public static class DirectionNamesConstants
	{
		public static string Import
		{
			get { return Res.GetString("653a9b42-1a0c-491b-a0fe-ad1d3c7b4966", "Import"); }
		}
		public static string Export
		{
			get { return Res.GetString("afcd4f89-e5f0-403f-b548-194057c592c5", "Export"); }
		}
		public static string BothImportAndExport
		{
			get { return Res.GetString("1eb2fce0-5d7f-438d-a1c6-b0ee70d1ab18", "Both Import and Export"); }
		}
		public static string Domestic
		{
			get { return Res.GetString("aa900f41-25a8-4caf-9d9f-c37abb41528a", "Domestic"); }
		}
		public const string CrossTrade = "CrossTrade";
		public static string AllForwarding
		{
			get { return Res.GetString("b3f66f08-3e1d-4ce0-baff-f668022dd6d8", "All Forwarding"); }
		}
		public static string Other
		{
			get { return Res.GetString("8cb25270-e3f7-48aa-83b1-ec68b8695ae6", "Other"); }
		}
	}

	public static class StmNoteContextUtils
	{
		public static StmNoteContexts GetContextFromTransportModeImportExport(string module, string direction, string transportMode, string containerMode)
		{
			StmNoteContexts result = new StmNoteContexts();

			result = FillStmNoteContextModule(result, module);
			result = FillStmNoteContextDirection(result, direction);
			result = FillStmNoteContextFreightMode(result, transportMode, containerMode);

			return result;
		}

		static StmNoteContexts FillStmNoteContextModule(StmNoteContexts result, string module)
		{
			if (module == Constants.GlobalModuleNamesConstants.Forwarding)
			{
				result.Module |= StmNoteContextModule.F;
				result.Module |= StmNoteContextModule.I;
				result.Module |= StmNoteContextModule.E;
			}

			if (module == Constants.GlobalModuleNamesConstants.CFS)
			{
				result.Module |= StmNoteContextModule.C;
				result.Module |= StmNoteContextModule.I;
			}
			if (module == Constants.GlobalModuleNamesConstants.Orders)
			{
				result.Module |= StmNoteContextModule.O;
				result.Module |= StmNoteContextModule.I;
			}
			if (module == Constants.GlobalModuleNamesConstants.Transport)
			{
				result.Module |= StmNoteContextModule.T;
			}
			if (module == Constants.GlobalModuleNamesConstants.Warehouse)
			{
				result.Module |= StmNoteContextModule.W;
			}
			if (module == Constants.GlobalModuleNamesConstants.ShipsAgency)
			{
				result.Module |= StmNoteContextModule.S;
			}
			if (module == Constants.GlobalModuleNamesConstants.CustomsDeclarations)
			{
				result.Module |= StmNoteContextModule.D;
				result.Module |= StmNoteContextModule.E;
			}

			return result;
		}

		static StmNoteContexts FillStmNoteContextDirection(StmNoteContexts result, string direction)
		{
			if (direction == DirectionNamesConstants.Import)
			{
				result.Direction |= StmNoteContextDirection.I;
				result.Direction |= StmNoteContextDirection.B;
			}
			else if (direction == DirectionNamesConstants.Export)
			{
				result.Direction |= StmNoteContextDirection.E;
				result.Direction |= StmNoteContextDirection.B;
			}
			else if (direction == DirectionNamesConstants.Domestic)
			{
				result.Direction |= StmNoteContextDirection.D;
			}
			else if (direction == DirectionNamesConstants.CrossTrade)
			{
				result.Direction |= StmNoteContextDirection.X;
			}
			else if (direction == DirectionNamesConstants.AllForwarding)
			{
				result.Direction |= StmNoteContextDirection.F;
			}
			else
			{
				result.Direction |= StmNoteContextDirection.O;
			}

			return result;
		}

		static StmNoteContexts FillStmNoteContextFreightMode(StmNoteContexts result, string transportMode, string containerMode)
		{
			if (transportMode == Enterprise.Core.Constants.TransportModes.Sea)
			{
				result.FreightMode |= StmNoteContextFreightMode.S;
			}
			if (transportMode == Enterprise.Core.Constants.TransportModes.Air)
			{
				result.FreightMode |= StmNoteContextFreightMode.I;
			}
			if (transportMode == Enterprise.Core.Constants.TransportModes.AirSea ||
				transportMode == Enterprise.Core.Constants.TransportModes.SeaAir)
			{
				result.FreightMode |= StmNoteContextFreightMode.B;
			}
			if (transportMode == Enterprise.Core.Constants.TransportModes.Road)
			{
				result.FreightMode |= StmNoteContextFreightMode.R;
			}
			if (transportMode == Enterprise.Core.Constants.TransportModes.Rail)
			{
				result.FreightMode |= StmNoteContextFreightMode.W;
			}

			if (containerMode == Enterprise.Core.Constants.ContainerModes.FCL)
			{
				result.FreightMode |= StmNoteContextFreightMode.F;
			}
			if (containerMode == Enterprise.Core.Constants.ContainerModes.LCL)
			{
				result.FreightMode |= StmNoteContextFreightMode.L;
			}

			return result;
		}

		public static StmNoteContexts StmNoteContextsAll
		{
			get
			{
				if (!fStmNoteContextsAllWasAlreadyCalledOnce)
				{
					fStmNoteContextsAll = new StmNoteContexts();
					fStmNoteContextsAll.Module |= StmNoteContextModule.A;
					fStmNoteContextsAll.Direction |= StmNoteContextDirection.A;
					fStmNoteContextsAll.FreightMode |= StmNoteContextFreightMode.A;
					fStmNoteContextsAllWasAlreadyCalledOnce = true;
				}
				return fStmNoteContextsAll;
			}
		}
		[ThreadStatic]
		static StmNoteContexts fStmNoteContextsAll;

		[ThreadStatic]
		static bool fStmNoteContextsAllWasAlreadyCalledOnce;

		public static string StmNoteContextsAllToString
		{
			get
			{
				return nameof(StmNoteContextModule.A) + nameof(StmNoteContextDirection.A) + nameof(StmNoteContextFreightMode.A);
			}
		}
	}
}
