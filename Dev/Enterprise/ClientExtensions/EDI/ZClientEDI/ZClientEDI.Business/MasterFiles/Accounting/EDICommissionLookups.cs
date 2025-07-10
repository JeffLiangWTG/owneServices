using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDICommissionLookups : CommissionLookups
	{
		#region New

		public new static EDICommissionLookups New(BusinessObjectFactory factory)
		{
			return new EDICommissionLookups(factory);
		}

		#endregion

		#region Register/Unregister SubType Override

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}

		#endregion

		#region Constructor

		protected EDICommissionLookups(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Products

		public override ReadOnlyCodeDescriptionPairList GetProducts()
		{
			var list = new CodeDescriptionPairList();
			list.AddRange(new ProductTypes());
			list.SortByDescription();

			return list;
		}

		#endregion

		#region Services

		public override ReadOnlyCodeDescriptionPairList GetServices(ZString product)
		{
			var list = new CodeDescriptionPairList();
			if (product == ProductTypes.Codes.Enterprise)
			{
				list.AddRange(BillingConstants.GetBillingSystemList());
			}

			AddPairsIfNotExist(list, EDIDataRegistry.Instance.BillingSystemChargeCodeMappings.Value.GetSystemCodes(product));

			list.SortByDescription();

			return list;
		}

		public override ReadOnlyCodeDescriptionPairList AllServices
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddRange(BillingConstants.GetBillingSystemList());

				AddPairsIfNotExist(list, EDIDataRegistry.Instance.BillingSystemChargeCodeMappings.Value.GetSystemCodes());

				list.SortByDescription();

				return list;
			}
		}

		#endregion

		#region SubModules

		public override ReadOnlyCodeDescriptionPairList GetSubModules(ZString product, ZString service)
		{
			return Factory.GetCachedValue("EDICommissionLookups.GetSubModules" + product + service, () =>
				{
					var list = new CodeDescriptionPairList();
					if (product == ProductTypes.Codes.Enterprise)
					{
						if (service == BillingConstants.BillingSystem.ODM)
						{
							list.AddRange(OdmModules);
						}
						else if (service == BillingConstants.BillingSystem.Fee)
						{
							list.AddRange(FeeTypes);
						}
						else if (service == BillingConstants.BillingSystem.STL)
						{
							list.AddRange(StlModules);
						}
						else if (service == BillingConstants.BillingSystem.Service)
						{
							list.AddRange(ServiceModules);
						}
					}

					AddPairsIfNotExist(list, EDIDataRegistry.Instance.BillingSystemChargeCodeMappings.Value.GetSubModuleCodes(product, service));

					list.SortByDescription();
					return list;
				});
		}

		public override ReadOnlyCodeDescriptionPairList AllSubModules
		{
			get
			{
				return Factory.GetCachedValue("EDICommissionLookups.AllSubModules", () =>
					{
						var list = new CodeDescriptionPairList();
						list.AddRange(OdmModules);
						list.AddRange(FeeTypes);
						list.AddRange(StlModules);
						list.AddRange(ServiceModules);

						AddPairsIfNotExist(list, EDIDataRegistry.Instance.BillingSystemChargeCodeMappings.Value.GetSubModuleCodes());
						list.SortByDescription();

						return list;
					});
			}
		}

		public CodeDescriptionPairList OdmModules
		{
			get
			{
				return Factory.GetCachedValue("EDICommissionLookups.OdmModules", () =>
				{
					return GetAllPriceItemCodes(BillingConstants.PriceHeaderType.ODM);
				});
			}
		}

		public CodeDescriptionPairList StlModules
		{
			get
			{
				return Factory.GetCachedValue("EDICommissionLookups.StlModules", () =>
				{
					return GetAllPriceItemCodes(BillingConstants.PriceHeaderType.STL);
				});
			}
		}

		public CodeDescriptionPairList ServiceModules
		{
			get
			{
				return Factory.GetCachedValue("EDICommissionLookups.ServiceModules", () =>
				{
					var result = new CodeDescriptionPairList();
					const string distinctPriceCodesSql = @"
SELECT
	CPS_Type, MAX(LTRIM(ISNULL(L7_Description, '')))
FROM 
	dbo.ClientPremiumService
	LEFT JOIN dbo.ClientLicencePriceItem ON CPS_PriceHeaderCode != '' AND CPS_Type = L7_Code AND L7_L6 IN (SELECT L6_PK FROM dbo.ClientLicencePriceHeader WHERE L6_SystemCode = CPS_PriceHeaderCode)
WHERE
	CPS_Type != ''
GROUP BY
	CPS_PriceHeaderCode, CPS_Type";
					using (var command = Db.Connection.Command(distinctPriceCodesSql))
					{
						using (var reader = command.ExecuteReader())
						{
							while (reader.Read())
							{
								result.Add(new CodeDescriptionPair((string)reader[0], (string)reader[1]));
							}
						}
					}

					return result;
				});
			}
		}

		static CodeDescriptionPairList GetAllPriceItemCodes(string systemCode)
		{
			var result = new CodeDescriptionPairList();
			const string distinctPriceCodesSql = @"
SELECT
	L7_Code,
	MAX(LTRIM(L7_Description))
FROM dbo.ClientLicencePriceItem
JOIN dbo.ClientLicencePriceHeader on L7_L6 = L6_PK
WHERE
	L7_Code <> ''
	AND L6_SystemCode = @L6_SystemCode
GROUP BY
	L7_Code";

			using (var command = Db.Connection.Command(distinctPriceCodesSql))
			{
				command.AddParameter("@L6_SystemCode", SqlDbType.VarChar, systemCode);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(new CodeDescriptionPair((string)reader[0], (string)reader[1]));
					}
				}
			}

			return result;
		}

		public CodeDescriptionPairList FeeTypes
		{
			get { return EDIDataRegistry.Instance.LicenceFeeTypes.Value.GetCodeDescriptionPairList(); }
		}

		static void AddPairsIfNotExist(CodeDescriptionPairList list, IEnumerable<ZString> codes)
		{
			if (codes.Any())
			{
				var existingCodes = new HashSet<string>(list.Cast<ICodeDescription>().Select(x => x.Code), StringComparer.OrdinalIgnoreCase);
				foreach (var code in codes)
				{
					if (!existingCodes.Contains(code))
					{
						list.AddPair(code, code);
						existingCodes.Add(code);
					}
				}
			}
		}

		#endregion

		#region ShouldShowServicesAndSubModules

		public override bool GetShouldShowServicesAndSubModules()
		{
			return true;
		}

		#endregion

		#region TriggerTypes

		protected override CodeDescriptionPairList GetNewTriggerTypes()
		{
			var list = base.GetNewTriggerTypes();
			list.AddRange(new EDICommissionTriggerTypes());
			return list;
		}

		#endregion
	}
}

