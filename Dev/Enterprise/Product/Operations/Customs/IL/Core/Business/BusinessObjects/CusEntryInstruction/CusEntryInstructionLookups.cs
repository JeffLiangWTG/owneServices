using System.Collections;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Business
{
	public class CusEntryInstructionLookups : Customs.Business.CusEntryInstructionLookups
	{
		public CusEntryInstructionLookups(AutoCusEntryInstruction parent) : base(parent)
		{
		}

		public ICodeDescriptionPairList ProcedureCodeList
			=> Factory.GetCachedValue("IL.CusEntryInstructionLookups.ProcedureCodeList",
				() => RefCusProcedureCollection.LoadCustomsProcedureCodesForCountryAndShipmentTypeAndGroup(
					factory: Factory,
					dataGroupingCode: Core.Constants.CountryCodes.Israel,
					shipmentType: GetShipmentType(),
					date: ZDateTime.Today,
					groupPatterns: new List<string>()));

		public ICollection AutonomyRegionTypeList
			=> GetAutonomyRegionTypeList();

		public OrgAddressDependentCollection FromWarehouseBondedWarehouseAddressList
		{
			get
			{
				if (Parent is CusEntryInstruction entryInstruction && entryInstruction.Warehouse?.Header is OrgHeader header)
				{
					return GetWarehouseBondedWarehouseAddressList(header);
				}
				return new OrgAddressDependentCollection(Factory);
			}
		}

		public OrgAddressDependentCollection ToWarehouseBondedWarehouseAddressList
		{
			get
			{
				if (Parent is CusEntryInstruction entryInstruction && entryInstruction.Warehouse2?.Header is OrgHeader header)
				{
					return GetWarehouseBondedWarehouseAddressList(header);
				}
				return new OrgAddressDependentCollection(Factory);
			}
		}

		public CodeDescriptionPairList PackageUQList => Factory.GetCachedValue<ILCustomsPackTypeList>();

		ICollection GetAutonomyRegionTypeList()
		{
			var list = ZZRefCusCodeListCombinedCollection.GetCachedCollection(
				Factory,
				Core.Constants.CountryCodes.Israel,
				codeType: Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILAutonomyRegionType,
				ZDateTime.Today);
			list.Load();
			return list;
		}

		OrgAddressDependentCollection GetWarehouseBondedWarehouseAddressList(OrgHeader header)
		{
			var fAddresses = new OrgAddressDependentCollection(Factory);
			if (header != null)
			{
				var filter = new ZQuery(OrgAddressSchema.OA_IsActive, SQLComparisonOperator.Equal, ZBool.True);
				fAddresses = new OrgAddressDependentCollection(header, filter);
				fAddresses.Load();
			}
			return fAddresses;
		}
	}
}
