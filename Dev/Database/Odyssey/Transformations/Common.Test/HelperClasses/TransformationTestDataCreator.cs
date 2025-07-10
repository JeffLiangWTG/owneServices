namespace Enterprise.DbUpgrader.Transformations
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.Linq;
	using CargoWise.Common;
	using CargoWise.Data;
	using Enterprise.DbUpgrader.Shared;
	using Enterprise.ZArchitecture.Schema;

	public class TransformationTestDataCreator
	{
		#region Create Records

		public Guid CreateStorageMain(Guid parentPK, string type, int db, DateTime lastActivity)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateStorageMainSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@type", SqlDbType.VarChar, StorageMainSchema.SM_Type.MaxLength, type);
				command.AddParameter("@db", SqlDbType.Int, db);
				command.AddParameter("@lastActivity", SqlDbType.DateTime, lastActivity);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid CreateStorageDoc(Guid storageMainPK, int db, string fileName, string desc, DateTime date, byte[] data, string docType = "")
		{
			Guid pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(string.Format(CultureInfo.InvariantCulture, CreateStorageDocSql, Db.DatabaseName.Trim(), db)))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, storageMainPK);
				command.AddParameter("@fileName", SqlDbType.VarChar, StorageDocsSchema.SC_FileName.MaxLength, fileName);
				command.AddParameter("@date", SqlDbType.DateTime, date);
				command.AddParameter("@data", SqlDbType.VarBinary, data);
				command.AddParameter("@docType", SqlDbType.Char, docType);
				command.AddParameter("@desc", SqlDbType.VarChar, StorageDocsSchema.SC_Desc.MaxLength, desc);
				command.AddParameter("@systemCreateTime", SqlDbType.DateTime, date);
				command.AddParameter("@lastEditTime", SqlDbType.DateTime, date);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		public void CreateOrgWithOfficeAddress(Guid clientPK, string code, string name)
		{
			CreateOrgOnly(clientPK, code, name);
			CreateOfficeAddress(clientPK, "test1", "test1");
		}

		public void CreateOrgOnly(Guid clientPK, string code, string name)
		{
			using (DbCommand command = Db.Connection.Command(CreateClientSql))
			{
				command.AddParameter("@clientPK", SqlDbType.UniqueIdentifier, clientPK);
				command.AddParameter("@code", SqlDbType.VarChar, OrgHeaderSchema.OH_Code.MaxLength, code);
				command.AddParameter("@name", SqlDbType.VarChar, OrgHeaderSchema.OH_FullName.MaxLength, name);
				command.ExecuteNonQuery();
			}
		}

		public Guid CreateOfficeAddress(Guid orgPK, string addressCode, string address1)
		{
			return CreateOrgAddressWithCapability(orgPK, addressCode, address1, "OFC");
		}

		public Guid CreateNotMainOfficeAddress(Guid orgPK, string addressCode, string address1)
		{
			return CreateOrgAddressWithCapability(orgPK, addressCode, address1, "OFC", false);
		}

		public Guid CreateOrgAddressWithCapability(Guid orgPK, string addressCode, string address1, string capabilityAddressType)
		{
			return CreateOrgAddressWithCapability(orgPK, addressCode, address1, capabilityAddressType, true);
		}

		Guid CreateOrgAddressWithCapability(Guid orgPK, string addressCode, string address1, string capabilityAddressType, bool isMain)
		{
			Guid addresspk = Guid.NewGuid();
			using (DbCommand command = Db.Connection.Command(CreateAddressSql))
			{
				command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, addresspk);
				command.AddParameter("@orgPK", SqlDbType.UniqueIdentifier, orgPK);
				command.AddParameter("@addressCode", SqlDbType.VarChar, OrgAddressSchema.OA_Code.MaxLength, addressCode);
				command.AddParameter("@address1", SqlDbType.VarChar, OrgAddressSchema.OA_Address1.MaxLength, address1);
				command.ExecuteNonQuery();
			}

			if (isMain)
			{
				CreateOrgAddressCapability(addresspk, capabilityAddressType, true);
			}
			else
			{
				CreateOrgAddressCapability(addresspk, capabilityAddressType, false);
			}

			return addresspk;
		}

		public Guid CreateOrgAddressCapability(Guid addressPK, string capabilityAddressType)
		{
			return CreateOrgAddressCapability(addressPK, capabilityAddressType, true);
		}

		Guid CreateOrgAddressCapability(Guid addressPK, string capabilityAddressType, bool isMain)
		{
			Guid addressCapabilityPK = Guid.NewGuid();
			using (DbCommand command = Db.Connection.Command(isMain ? CreateOrgAddressCapabilitySql : CreateNotMainOrgAddressCapabilitySql))
			{
				command.AddParameter("@pzPK", SqlDbType.UniqueIdentifier, addressCapabilityPK);
				command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, addressPK);
				command.AddParameter("@capabilityAddressType", SqlDbType.VarChar, OrgAddressCapabilitySchema.PZ_AddressType.MaxLength, capabilityAddressType);
				command.ExecuteNonQuery();
			}
			return addressCapabilityPK;
		}

		public Guid CreateOrgAppointedAgentPort(Guid orgPK, string port, string air, string sea, string rail, string road, Guid addrPK, string direction = "BTH", string seaAirCarrierOrForwarderType = "FWD")
		{
			var agentPortPK = Guid.NewGuid();
			using (DbCommand command = Db.Connection.Command(CreateOrgAppointedAgentPorts))
			{
				command.AddParameter("@O5_PK", SqlDbType.UniqueIdentifier, agentPortPK);
				command.AddParameter("@O5_PortOrCountry", SqlDbType.VarChar, OrgAppointedAgentPortsSchema.O5_PortOrCountry.MaxLength, port);
				command.AddParameter("@O5_SeaAgentStatus", SqlDbType.VarChar, OrgAppointedAgentPortsSchema.O5_SeaAgentStatus.MaxLength, sea);
				command.AddParameter("@O5_AirAgentStatus", SqlDbType.VarChar, OrgAppointedAgentPortsSchema.O5_AirAgentStatus.MaxLength, air);
				command.AddParameter("@O5_RailAgentStatus", SqlDbType.VarChar, OrgAppointedAgentPortsSchema.O5_RailAgentStatus.MaxLength, rail);
				command.AddParameter("@O5_RoadAgentStatus", SqlDbType.VarChar, OrgAppointedAgentPortsSchema.O5_RoadAgentStatus.MaxLength, road);
				command.AddParameter("@O5_OA_AgentOfficeAddress", SqlDbType.UniqueIdentifier, addrPK);
				command.AddParameter("@O5_OH", SqlDbType.UniqueIdentifier, orgPK);
				command.AddParameter("@O5_SeaAirCarrierOrForwarderType", SqlDbType.VarChar, OrgAppointedAgentPortsSchema.O5_SeaAirCarrierOrForwarderType.MaxLength, seaAirCarrierOrForwarderType);
				command.AddParameter("@O5_TerminalType", SqlDbType.VarChar, OrgAppointedAgentPortsSchema.O5_TerminalType.MaxLength, string.Empty);
				command.AddParameter("@O5_ContainerType", SqlDbType.Int, 0);
				command.AddParameter("@O5_AgentDirection", SqlDbType.VarChar, OrgAppointedAgentPortsSchema.O5_AgentDirection.MaxLength, direction);
				command.ExecuteNonQuery();
			}
			return agentPortPK;
		}

		public Guid CreateOrgParkContainerType(Guid agentPortPK, string containerStorageClass)
		{
			var orgParkContainerTypePK = Guid.NewGuid();
			using (DbCommand command = Db.Connection.Command(CreateOrgParkContainerTypeSql))
			{
				command.AddParameter("@PT_PK", SqlDbType.UniqueIdentifier, orgParkContainerTypePK);
				command.AddParameter("@containerStorageClass", SqlDbType.VarChar, OrgParkContainerTypeSchema.PT_ContainerStorageClass.MaxLength, containerStorageClass);
				command.AddParameter("@agentPortPK", SqlDbType.UniqueIdentifier, agentPortPK);
				command.ExecuteNonQuery();
			}
			return orgParkContainerTypePK;
		}

		public void CreateEquipment(Guid equipmentPK, string code)
		{
			using (DbCommand command = Db.Connection.Command(CreateEquipmentSql))
			{
				command.AddParameter("@equipmentPK", SqlDbType.UniqueIdentifier, equipmentPK);
				command.AddParameter("@code", SqlDbType.VarChar, code);
				command.ExecuteNonQuery();
			}
		}

		#region CreateCompany

		public Guid CreateCompany(string companyCode, string countryCode, Guid? orgProxyPK = null)
		{
			return CreateCompany(Guid.NewGuid(), companyCode, countryCode, orgProxyPK);
		}

		public Guid CreateCompany(Guid companyPK, string companyCode, string countryCode, Guid? orgProxyPK = null)
			=> CreateCompany(companyPK, companyCode, countryCode, "AUD", orgProxyPK);

		public Guid CreateCompany(Guid companyPK, string companyCode, string countryCode, string currencyCode, Guid? orgProxyPK = null)
		{
			using (DbCommand command = Db.Connection.Command(CreateCompanySql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@companyCode", SqlDbType.VarChar, companyCode);
				command.AddParameter("@countryCode", SqlDbType.VarChar, countryCode);
				command.AddParameter("@currencyNK", SqlDbType.VarChar, currencyCode);
				command.AddParameter("@orgProxyPK", SqlDbType.UniqueIdentifier, orgProxyPK == null ? DBNull.Value : orgProxyPK);
				command.ExecuteNonQuery();
			}
			return companyPK;
		}

		public Guid GetGlbGroupGuidFromCode(string code)
		{
			return GetColumnFromTableByValue("GG_PK", "dbo.GlbGroup", "GG_Code", code);
		}

		Guid GetColumnFromTableByValue(string selectColumn, string table, string valueColumn, string value)
		{
			return (Guid)Db.Connection.ExecuteScalar("SELECT " + selectColumn + " FROM " + table + " WHERE " + valueColumn + " = '" + value + "'");
		}

		#endregion

		public Guid CreateBranch(string branchCode, string homePort, Guid companyPK)
		{
			var pk = Guid.NewGuid();
			CreateBranch(pk, branchCode, homePort, companyPK);
			return pk;
		}

		public void CreateBranch(Guid branchPK, string branchCode, string homePort, Guid companyPK)
		{
			CreateBranch(branchPK, branchCode, homePort, companyPK, GetOrganisation());
		}

		public void CreateBranch(Guid branchPK, string branchCode, string homePort, Guid companyPK, Guid orgProxyPk)
		{
			using (var command = Db.Connection.Command(CreateBranchSql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@branchCode", SqlDbType.VarChar, GlbBranchSchema.GB_Code.MaxLength, branchCode);
				command.AddParameter("@homePort", SqlDbType.VarChar, GlbBranchSchema.GB_RL_NKHomePort.MaxLength, homePort);
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@OrgProxy", SqlDbType.UniqueIdentifier, orgProxyPk);
				command.ExecuteNonQuery();
			}
		}

		public Guid GetOrganisation()
		{
			return (Guid)Db.Connection.ExecuteScalar("SELECT TOP 1 OH_PK FROM dbo.OrgHeader");
		}

		public void CreateJobHeader(Guid jobHeaderPK, Guid parentPK, string parentTableCode, string jobNum, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			CreateJobHeader(jobHeaderPK, parentPK, parentTableCode, jobNum, companyPK, branchPK, departmentPK, Guid.Empty);
		}

		public void CreateJobHeader(Guid jobHeaderPK, Guid parentPK, string parentTableCode, string jobNum, Guid companyPK, Guid branchPK, Guid departmentPK, Guid parentJobPK, string clientContractNum = null)
		{
			using (DbCommand command = Db.Connection.Command(CreateJobHeaderSql))
			{
				command.AddParameter("@jobHeaderPK", SqlDbType.UniqueIdentifier, jobHeaderPK);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, JobHeaderSchema.JH_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@jobNum", SqlDbType.VarChar, JobHeaderSchema.JH_JobNum.MaxLength, jobNum);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				command.AddParameter("@parentJobPK", SqlDbType.UniqueIdentifier, parentJobPK != Guid.Empty ? parentJobPK : DBNull.Value);
				command.AddParameter("@clientContractNum", SqlDbType.VarChar, JobHeaderSchema.JH_ClientContractNumber.MaxLength, clientContractNum == null ? DBNull.Value : clientContractNum);
				command.ExecuteNonQuery();
			}
		}

		public void CreateJobCharge(Guid jobChargePK, Guid jobHeaderPK, Guid chargeCodePK, Guid branchPK, Guid departmentPK)
		{
			using (DbCommand command = Db.Connection.Command(CreateJobChargeSql))
			{
				command.AddParameter("@jobChargePK", SqlDbType.UniqueIdentifier, jobChargePK);
				command.AddParameter("@jobHeaderPK", SqlDbType.UniqueIdentifier, jobHeaderPK);
				command.AddParameter("@chargeCodePK", SqlDbType.UniqueIdentifier, chargeCodePK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				command.ExecuteNonQuery();
			}
		}

		public void CreateChargeCode(Guid chargeCodePK, string code, string chargeType, Guid revenueAccount, Guid wIPAccount, Guid costAccount, Guid accrualAccount, Guid company)
		{
			CreateChargeCode(chargeCodePK, code, chargeType, revenueAccount, wIPAccount, costAccount, accrualAccount, company, Guid.Empty);
		}

		public void CreateChargeCode(Guid chargeCodePK, string code, string chargeType, Guid revenueAccount, Guid wIPAccount, Guid costAccount, Guid accrualAccount, Guid company, Guid gSTRate)
		{
			using (DbCommand command = Db.Connection.Command(CreateChargeCodeSql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, chargeCodePK);
				command.AddParameter("@Code", SqlDbType.Char, AccChargeCodeSchema.AC_Code.MaxLength, code);
				command.AddParameter("@ChargeType", SqlDbType.Char, AccChargeCodeSchema.AC_ChargeType.MaxLength, chargeType);
				command.AddParameter("@RevenueAccount", SqlDbType.UniqueIdentifier, revenueAccount != Guid.Empty ? revenueAccount : DBNull.Value);
				command.AddParameter("@WIPAccount", SqlDbType.UniqueIdentifier, wIPAccount != Guid.Empty ? wIPAccount : DBNull.Value);
				command.AddParameter("@CostAccount", SqlDbType.UniqueIdentifier, costAccount != Guid.Empty ? costAccount : DBNull.Value);
				command.AddParameter("@AccrualAccount", SqlDbType.UniqueIdentifier, accrualAccount != Guid.Empty ? accrualAccount : DBNull.Value);
				command.AddParameter("@Company", SqlDbType.UniqueIdentifier, company);
				command.AddParameter("@GSTRate", SqlDbType.UniqueIdentifier, gSTRate != Guid.Empty ? gSTRate : DBNull.Value);
				command.ExecuteNonQuery();
			}
		}

		public void CreateShipment(Guid shipmentPK, string shipmentNumber, bool cancelled = false, bool isBooking = false, string shipmentType = "", bool isHighRisk = false, string origin = "AUSYD")
		{
			using (DbCommand command = Db.Connection.Command(CreateShipmentSql))
			{
				command.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@shipmentNumber", SqlDbType.VarChar, JobShipmentSchema.JS_UniqueConsignRef.MaxLength, shipmentNumber);
				command.AddParameter("@shipmentType", SqlDbType.VarChar, JobShipmentSchema.JS_ShipmentType.MaxLength, shipmentType);
				command.AddParameter("@cancelled", SqlDbType.Bit, cancelled);
				command.AddParameter("@isBooking", SqlDbType.Bit, isBooking);
				command.AddParameter("@isHighRisk", SqlDbType.Bit, isHighRisk);
				command.AddParameter("@origin", SqlDbType.VarChar, origin);
				command.ExecuteNonQuery();
			}
		}

		public void CreateDocsAndCartage(Guid docsAndCartagePK, string parentTableCode, Guid parentID, DateTime? actualDeliveryTime = null)
		{
			using (DbCommand command = Db.Connection.Command(CreateDocsAndCartageSql))
			{
				command.AddParameter("@docsAndCartagePK", SqlDbType.UniqueIdentifier, docsAndCartagePK);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, JobDocsAndCartageSchema.JP_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@actualDeliveryTime", SqlDbType.SmallDateTime, actualDeliveryTime == null ? DBNull.Value : actualDeliveryTime);
				command.ExecuteNonQuery();
			}
		}

		public void CreateDeclaration(Guid declarationPK, string declarationNumber, int clusterKey, Guid? shipmentPK = null, bool cancelled = false)
		{
			CreateDeclaration(declarationPK, declarationNumber, clusterKey, Guid.Empty, Guid.Empty, shipmentPK, cancelled);
		}

		public void CreateDeclarationWithAddInfo(Guid declarationPK, string declarationNumber, Guid branchPK, Guid companyPK, string addInfo, int clusterKey, Guid? shipmentPK = null, bool cancelled = false)
		{
			CreateDeclaration(declarationPK, declarationNumber, clusterKey, branchPK, companyPK, shipmentPK, cancelled, addInfo);
		}

		public void CreateDeclaration(Guid declarationPK, string declarationNumber, int clusterKey, Guid branchPK, Guid companyPK, Guid? shipmentPK = null, bool cancelled = false, string addInfo = "", string messageType = "", string dataModel = "DE")
		{
			using (DbCommand command = Db.Connection.Command(CreateDeclarationSql))
			{
				command.AddParameterBasedOnDbColumn("@declarationPK", declarationPK, JobDeclarationSchema.PK);
				command.AddParameterBasedOnDbColumn("@declarationNumber", declarationNumber, JobDeclarationSchema.JE_DeclarationReference);
				command.AddParameterBasedOnDbColumn("@branchPK", branchPK, JobDeclarationSchema.JE_GB);
				command.AddParameterBasedOnDbColumn("@companyPK", companyPK, JobDeclarationSchema.JE_GC);
				command.AddParameterBasedOnDbColumn("@cancelled", cancelled, JobDeclarationSchema.JE_IsCancelled);
				command.AddParameterBasedOnDbColumn("@shipmentPK", shipmentPK ?? SqlGuid.Null, JobDeclarationSchema.JE_JS);
				command.AddParameterBasedOnDbColumn("@addInfo", addInfo, JobDeclarationSchema.JE_AddInfo);
				command.AddParameterBasedOnDbColumn("@messageType", messageType, JobDeclarationSchema.JE_MessageType);
				command.AddParameterBasedOnDbColumn("@clusterKey", clusterKey, JobDeclarationSchema.JE_ClusterKey);
				command.AddParameterBasedOnDbColumn("@dataModel", dataModel, JobDeclarationSchema.JE_DataModel);
				command.ExecuteNonQuery();
			}
		}

		public void CreateContainerLeg_Old(Guid containerLegPK, string leg, Guid cartagePK)
		{
			CreateContainerLeg_Old(containerLegPK, leg, cartagePK, Guid.Empty, Guid.Empty, Guid.Empty);
		}

		public void CreateContainerLeg_Old(Guid containerLegPK, string leg, Guid cartagePK, Guid pickupAddressPK, Guid waitPointAddressPK, Guid deliveryAddressPK)
		{
			CreateContainerLeg_Old(containerLegPK, leg, cartagePK, pickupAddressPK, waitPointAddressPK, deliveryAddressPK, "", "", "", "", Guid.Empty, Guid.Empty, Guid.Empty, "", "", DateTime.Today, DateTime.Today, "", "");
		}

		public void CreateContainerLeg_Old(Guid containerLegPK, string leg, Guid cartagePK, Guid pickupAddressPK, Guid waitPointAddressPK, Guid deliveryAddressPK,
			string transName, string licence, string driver, string rego, Guid transCoPK, Guid staffPK, Guid vehiclePK,
			string drop, string signed, DateTime pickupTime, DateTime deliverTime, string gatePassNumber, string splitDeliverySuffix)
		{
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "JobContainerLegs", "JU_Leg", "varchar(3)", "''");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "JobContainerLegs", "JU_JJ", "uniqueidentifier");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "JobContainerLegs", "JU_TransportCoName", "varchar(20)", "''");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "JobContainerLegs", "JU_DriversLicense", "varchar(20)", "''");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "JobContainerLegs", "JU_DriversName", "varchar(20)", "''");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "JobContainerLegs", "JU_TruckRegistration", "varchar(20)", "''");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "JobContainerLegs", "JU_OH_TransportCo", "uniqueidentifier");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "JobContainerLegs", "JU_GS", "uniqueidentifier");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "JobContainerLegs", "JU_RQ_Vehicle", "uniqueidentifier");
			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, "JobContainerLegs", "JU_DropMode", "varchar(3)", "''");

			using (DbCommand command = Db.Connection.Command(CreateContainerLegSql))
			{
				command.AddParameter("@containerLegPK", SqlDbType.UniqueIdentifier, containerLegPK);
				command.AddParameter("@leg", SqlDbType.VarChar, 3, leg);
				command.AddParameter("@cartagePK", SqlDbType.UniqueIdentifier, cartagePK == Guid.Empty ? DBNull.Value : cartagePK);
				command.AddParameter("@pickupAddressPK", SqlDbType.UniqueIdentifier, pickupAddressPK == Guid.Empty ? DBNull.Value : pickupAddressPK);
				command.AddParameter("@waitPointAddressPK", SqlDbType.UniqueIdentifier, waitPointAddressPK == Guid.Empty ? DBNull.Value : waitPointAddressPK);
				command.AddParameter("@deliveryAddressPK", SqlDbType.UniqueIdentifier, deliveryAddressPK == Guid.Empty ? DBNull.Value : deliveryAddressPK);

				command.AddParameter("@transName", SqlDbType.VarChar, 20, transName);
				command.AddParameter("@licence", SqlDbType.VarChar, 20, licence);
				command.AddParameter("@driver", SqlDbType.VarChar, 20, driver);
				command.AddParameter("@rego", SqlDbType.VarChar, 20, rego);
				command.AddParameter("@transCoPK", SqlDbType.UniqueIdentifier, transCoPK == Guid.Empty ? DBNull.Value : transCoPK);
				command.AddParameter("@staffPK", SqlDbType.UniqueIdentifier, staffPK == Guid.Empty ? DBNull.Value : staffPK);
				command.AddParameter("@vehiclePK", SqlDbType.UniqueIdentifier, vehiclePK == Guid.Empty ? DBNull.Value : vehiclePK);
				command.AddParameter("@drop", SqlDbType.VarChar, 3, drop);
				command.AddParameter("@signed", SqlDbType.VarChar, JobContainerLegsSchema.JU_DeliverySignedFor.MaxLength, signed);
				command.AddParameter("@pickupTime", SqlDbType.DateTime, pickupTime);
				command.AddParameter("@deliverTime", SqlDbType.DateTime, deliverTime);
				command.AddParameter("@gatePassNumber", SqlDbType.VarChar, JobContainerLegsSchema.JU_GatePassNumber.MaxLength, gatePassNumber);
				command.AddParameter("@splitDeliverySuffix", SqlDbType.VarChar, JobContainerLegsSchema.JU_SplitDeliverySuffix.MaxLength, splitDeliverySuffix);
				command.ExecuteNonQuery();
			}
		}

		public void CreateContainer(Guid containerPK, string containerNum, Guid consolPK, Guid allocationLinePK = default(Guid))
		{
			using (DbCommand command = Db.Connection.Command(CreateContainerSql))
			{
				command.AddParameter("@containerPK", SqlDbType.UniqueIdentifier, containerPK);
				command.AddParameter("@containerNum", SqlDbType.VarChar, JobContainerSchema.JC_ContainerNum.MaxLength, containerNum);
				command.AddParameter("@consolPK", SqlDbType.UniqueIdentifier, consolPK == Guid.Empty ? DBNull.Value : consolPK);
				command.AddParameter("@allocationLinePK", SqlDbType.UniqueIdentifier, allocationLinePK == Guid.Empty ? DBNull.Value : allocationLinePK);
				command.ExecuteNonQuery();
			}
		}

		public void CreatePackLine(Guid packLinePK, Guid shipmentPK, int packs, decimal weight, decimal volume)
		{
			using (DbCommand command = Db.Connection.Command(CreatePackLineSql))
			{
				command.AddParameter("@packLinePK", SqlDbType.UniqueIdentifier, packLinePK);
				command.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, shipmentPK == Guid.Empty ? DBNull.Value : shipmentPK);
				command.AddParameter("@packs", SqlDbType.Int, packs);
				command.AddParameter("@weight", SqlDbType.Decimal, weight);
				command.AddParameter("@volume", SqlDbType.Decimal, volume);
				command.ExecuteNonQuery();
			}
		}

		public void CreateConsol(Guid consolPK, string consolNumber, bool cancelled = false, Guid allocationLinePK = default(Guid), bool overrideWaybillDefaults = false, string loadPort = "AUSYD", string dischargePort = "USLAX", string transportMode = "AIR")
		{
			using (DbCommand command = Db.Connection.Command(CreateConsolSql))
			{
				command.AddParameter("@consolPK", SqlDbType.UniqueIdentifier, consolPK);
				command.AddParameter("@consolNumber", SqlDbType.VarChar, JobConsolSchema.JK_UniqueConsignRef.MaxLength, consolNumber);
				command.AddParameter("@cancelled", SqlDbType.Bit, cancelled);
				command.AddParameter("@allocationLinePK", SqlDbType.UniqueIdentifier, allocationLinePK == Guid.Empty ? DBNull.Value : allocationLinePK);
				command.AddParameter("@overrideWaybillDefaults", SqlDbType.Bit, overrideWaybillDefaults);
				command.AddParameter("@loadPort", SqlDbType.VarChar, loadPort);
				command.AddParameter("@dischargePort", SqlDbType.VarChar, dischargePort);
				command.AddParameter("@transportMode", SqlDbType.VarChar, transportMode);
				command.ExecuteNonQuery();
			}
		}

		public void CreateJobConShipLink(Guid consolPK, Guid shipmentPK)
		{
			using (DbCommand command = Db.Connection.Command(CreateJobConShipLinkSql))
			{
				command.AddParameter("@linkPK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@consolPK", SqlDbType.UniqueIdentifier, consolPK);
				command.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, shipmentPK);
				command.ExecuteNonQuery();
			}
		}

		public Guid CreateJobConsolTransport(Guid shipmentPK, string parentType)
		{
			var jwPK = Guid.NewGuid();
			using (DbCommand command = Db.Connection.Command(CreateJobConsolTransportSql))
			{
				command.AddParameter("@jwPK", SqlDbType.UniqueIdentifier, jwPK);
				command.AddParameter("@parentGUID", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@parentType", SqlDbType.VarChar, parentType);
				command.ExecuteNonQuery();
			}

			return jwPK;
		}

		public void CreateDepartment(Guid departmentPK, string departmentCode)
		{
			using (DbCommand command = Db.Connection.Command(CreateDepartmentSql))
			{
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				command.AddParameter("@departmentCode", SqlDbType.VarChar, GlbDepartmentSchema.GE_Code.MaxLength, departmentCode);
				command.ExecuteNonQuery();
			}
		}

		public void CreateDocAddress(Guid docAddressPK, Guid parentPK, string parentTableCode, string addressType, string companyName, string regNumType = "", string regNum = "", bool addressOverride = false)
		{
			CreateDocAddress(docAddressPK, parentPK, parentTableCode, addressType, companyName, 0, regNumType: regNumType, regNum: regNum, addressOverride: addressOverride);
		}

		public void CreateDocAddress(Guid docAddressPK, Guid parentPK, string parentTableCode, string addressType, string companyName, int addressSeq, Guid? addressPK = null, string regNumType = "", string regNum = "", bool addressOverride = false)
		{
			using (DbCommand command = Db.Connection.Command(CreateDocAddressSql))
			{
				command.AddParameter("@PK", SqlDbType.UniqueIdentifier, docAddressPK);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, JobDocAddressSchema.E2_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@AddressType", SqlDbType.VarChar, JobDocAddressSchema.E2_AddressType.MaxLength, addressType);
				command.AddParameter("@CompanyName", SqlDbType.VarChar, JobDocAddressSchema.E2_CompanyName.MaxLength, companyName);
				command.AddParameter("@addressSeq", SqlDbType.TinyInt, addressSeq);
				command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, addressPK == null ? DBNull.Value : addressPK);
				command.AddParameter("@govRegNumTyp", SqlDbType.VarChar, regNumType);
				command.AddParameter("@govRegNum", SqlDbType.VarChar, regNum);
				command.AddParameter("@AddressOverride", SqlDbType.Bit, addressOverride);
				command.AddParameter("@ValidationStatus", SqlDbType.VarChar, addressOverride ? "MAN" : "NRQ");
				command.ExecuteNonQuery();
			}
		}

		public Guid CreateContact(string contactName, string orgCode, string orgFullName, Guid? personPk = null)
		{
			return CreateContact(Guid.NewGuid(), contactName, orgCode, orgFullName, personPk);
		}

		public Guid CreateContact(Guid contactPK, string contactName, string orgCode, string orgFullName, Guid? personPk)
		{
			var orgPK = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, orgPK);
				command.AddParameter("@code", SqlDbType.VarChar, OrgHeaderSchema.OH_Code.MaxLength, orgCode);
				command.AddParameter("@fullName", SqlDbType.VarChar, OrgHeaderSchema.OH_FullName.MaxLength, orgFullName);
				command.AddParameter("@isConsignor", SqlDbType.Bit, false);
				command.AddParameter("@isForwarder", SqlDbType.Bit, false);
				command.ExecuteNonQuery();
			}

			using (var command = Db.Connection.Command(CreateContactSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, contactPK);
				command.AddParameter("@name", SqlDbType.VarChar, OrgContactSchema.OC_ContactName.MaxLength, contactName);
				command.AddParameter("@orgPk", SqlDbType.UniqueIdentifier, orgPK);
				command.AddParameter("@personPk", SqlDbType.UniqueIdentifier, personPk ?? (object)DBNull.Value);
				command.AddParameter("@email", SqlDbType.VarChar, CreateContactEmail(contactName));

				command.ExecuteNonQuery();
			}

			return contactPK;
		}

		public void CreateContact(Guid contactPK, string contactName, Guid orgPK)
		{
			using (var command = Db.Connection.Command(CreateContactSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, contactPK);
				command.AddParameter("@name", SqlDbType.VarChar, OrgContactSchema.OC_ContactName.MaxLength, contactName);
				command.AddParameter("@orgPk", SqlDbType.UniqueIdentifier, orgPK);
				command.AddParameter("@personPk", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@email", SqlDbType.VarChar, CreateContactEmail(contactName));
				command.ExecuteNonQuery();
			}
		}

		public Guid CreateContact(string contactName, Guid orgPK)
		{
			var contactPk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateContactSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, contactPk);
				command.AddParameter("@name", SqlDbType.VarChar, OrgContactSchema.OC_ContactName.MaxLength, contactName);
				command.AddParameter("@orgPk", SqlDbType.UniqueIdentifier, orgPK);
				command.AddParameter("@personPk", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("@email", SqlDbType.VarChar, CreateContactEmail(contactName));
				command.ExecuteNonQuery();
			}

			return contactPk;
		}

		string CreateContactEmail(string contactName) => string.Concat(contactName.Where(c => char.IsLetter(c))) + "@wisetech.com";

		public Guid CreateContact(string contactName, Guid orgPK, string jobCategoryCode)
		{
			var contactPk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateContactWithJobCategorySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, contactPk);
				command.AddParameter("@name", SqlDbType.VarChar, OrgContactSchema.OC_ContactName.MaxLength, contactName);
				command.AddParameter("@orgPk", SqlDbType.UniqueIdentifier, orgPK);
				command.AddParameter("@jobCategory", SqlDbType.VarChar, jobCategoryCode);
				command.ExecuteNonQuery();
			}

			return contactPk;
		}

		public Guid CreateStaff(string loginName, string code)
		{
			return CreateStaff(Guid.NewGuid(), loginName, code);
		}

		public Guid CreateStaff(Guid staffPK, string loginName, string code)
		{
			using (DbCommand command = Db.Connection.Command(CreateStaffSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, staffPK);
				command.AddParameter("@loginName", SqlDbType.VarChar, GlbStaffSchema.GS_LoginName.MaxLength, loginName);
				command.AddParameter("@code", SqlDbType.VarChar, GlbStaffSchema.GS_Code.MaxLength, code);
				command.ExecuteNonQuery();
			}

			return staffPK;
		}

		public Guid CreateOrgStaffAssignments(string staffFullName, string staffCode, Guid orgHeaderPk, string role)
		{
			var pk = Guid.NewGuid();
			CreateStaff(staffFullName, staffCode);

			using (var command = Db.Connection.Command(CreateOrgStaffAssignmentsSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@role", SqlDbType.VarChar, role);
				command.AddParameter("@department", SqlDbType.VarChar, "ALL");
				command.AddParameter("@orgHeaderPk", SqlDbType.UniqueIdentifier, orgHeaderPk);
				command.AddParameter("@personResponsible", SqlDbType.VarChar, staffCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid CreateHRJobApplicant(string emailAddress, Guid personPK)
		{
			var applicantPK = Guid.NewGuid();
			using (DbCommand command = Db.Connection.Command(CreateHRJobApplicantSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, applicantPK);
				command.AddParameter("@emailAddress", SqlDbType.VarChar, HRJobApplicantSchema.HA_EmailAddress.MaxLength, emailAddress);
				command.AddParameter("@personPk", SqlDbType.UniqueIdentifier, personPK);
				command.ExecuteNonQuery();
			}

			return applicantPK;
		}

		public Guid CreatePerson(string fullName)
		{
			var personPK = Guid.NewGuid();
			using (DbCommand command = Db.Connection.Command(CreatePersonSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, personPK);
				command.AddParameter("@fullName", SqlDbType.VarChar, GlbPersonSchema.PER_FullName.MaxLength, fullName);
				command.ExecuteNonQuery();
			}

			return personPK;
		}

		public Guid CreateStmALog(string tableName, Guid parentPK, string userCode, string eventCode)
		{
			return CreateStmALog(tableName, parentPK, userCode, eventCode, "");
		}

		public Guid CreateStmALog(string tableName, Guid parentPK, string userCode, string eventCode, string reference)
		{
			return CreateStmALog(tableName, parentPK, userCode, eventCode, reference, DateTime.Now, DateTime.UtcNow);
		}

		public Guid CreateStmALog(string tableName, Guid parentPK, string userCode, string eventCode, string reference, DateTime date, DateTime utcDate, bool cancelled = false)
		{
			var pk = Guid.NewGuid();
			using (DbCommand command = Db.Connection.Command(CreateStmALogSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@tableName", SqlDbType.VarChar, StmALogSchema.SL_Table.MaxLength, tableName);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@userCode", SqlDbType.VarChar, StmALogSchema.SL_GS_NKUser.MaxLength, userCode);
				command.AddParameter("@cancelled", SqlDbType.VarChar, StmALogSchema.SL_IsCancelled.MaxLength, cancelled ? "Y" : "N");
				command.AddParameter("@eventCode", SqlDbType.VarChar, StmALogSchema.SL_SE_NKEvent.MaxLength, eventCode);
				command.AddParameter("@reference", SqlDbType.VarChar, StmALogSchema.SL_Reference.MaxLength, reference);
				command.AddParameter("@date", SqlDbType.SmallDateTime, date);
				command.AddParameter("@utcDate", SqlDbType.SmallDateTime, utcDate);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public void CreateStmJobQueue(DateTime postedTimeUtc, string filterName, string status, Guid parentID, Guid logReference, DateTime eventTime, string parentTableCode, DateTime? eventTimeUtc = null)
		{
			using (DbCommand command = Db.Connection.Command(CreateStmJobQueueSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@postedTimeUtc", SqlDbType.SmallDateTime, postedTimeUtc);
				command.AddParameter("@filterName", SqlDbType.VarChar, StmJobQueueSchema.SJ_FilterName.MaxLength, filterName);
				command.AddParameter("@status", SqlDbType.Char, StmJobQueueSchema.SJ_Status.MaxLength, status);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@logReference", SqlDbType.UniqueIdentifier, logReference);
				command.AddParameter("@eventTime", SqlDbType.SmallDateTime, eventTime);
				command.AddParameter("@eventTimeUtc", SqlDbType.SmallDateTime, eventTimeUtc ?? eventTime.ToUniversalTime());
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, StmJobQueueSchema.SJ_ParentTableCode.MaxLength, parentTableCode);
				command.ExecuteNonQuery();
			}
		}

		public Guid CreateSpotQuoteHeader(string quoteNumber)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateSpotQuoteHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@quoteNumber", SqlDbType.VarChar, RatingHeaderSchema.TH_QuoteNumber.MaxLength, quoteNumber);
				command.AddParameter("@quoteDateTime", SqlDbType.Date, DateTime.Today.AddMonths(-6));
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid CreateOneOffShipment(Guid ratingHeaderPK)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateOneOffShipmentSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@ratingHeaderPK", SqlDbType.UniqueIdentifier, ratingHeaderPK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid CreateOneOffContainer(Guid oneOffShipmentPK, string packType, string unitOfDimension, string unitOfVolume, string unitOfWeight)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateOneOffContainersSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@oneOffShipmentPK", SqlDbType.UniqueIdentifier, oneOffShipmentPK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid CreateRateTariffDiscount(Guid ratingHeaderPK)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRateTariffDiscountSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@ratingHeaderPK", SqlDbType.UniqueIdentifier, ratingHeaderPK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid CreateRateAttachmentSet()
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRateAttachmentSetSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid CreateRateAttachment(Guid ratingHeaderPK, Guid rateAttachmentSetPK)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRateAttachmentSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@ratingHeaderPK", SqlDbType.UniqueIdentifier, ratingHeaderPK);
				command.AddParameter("@rateAttachmentSetPK", SqlDbType.UniqueIdentifier, rateAttachmentSetPK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid CreateOrgCusCode(Guid orgPk, string type, string customsRegNo, string countryCode)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgCusCodeSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@CustomsRegNo", SqlDbType.NVarChar, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength, customsRegNo);
				command.AddParameter("@Type", SqlDbType.Char, OrgCusCodeSchema.OK_CodeType.MaxLength, type);
				command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, orgPk);
				command.AddParameter("@CountryCode", SqlDbType.VarChar, OrgCusCodeSchema.OK_RN_NKCodeCountry.MaxLength, countryCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public void CreateOrgCusCodeWithPremiseAddress(Guid orgPk, string type, string customsRegNo, string countryCode, Guid premiseAddressPK)
		{
			using (var command = Db.Connection.Command(CreateOrgCusCodeWithPremiseSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@CustomsRegNo", SqlDbType.NVarChar, OrgCusCodeSchema.OK_CustomsRegNo.MaxLength, customsRegNo);
				command.AddParameter("@Type", SqlDbType.Char, OrgCusCodeSchema.OK_CodeType.MaxLength, type);
				command.AddParameter("@OrgPk", SqlDbType.UniqueIdentifier, orgPk);
				command.AddParameter("@CountryCode", SqlDbType.VarChar, OrgCusCodeSchema.OK_RN_NKCodeCountry.MaxLength, countryCode);
				command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, premiseAddressPK);
				command.ExecuteNonQuery();
			}
		}

		public Guid CreateSupplierBookingHeader(Guid orgAddressPK)
		{
			var headerPK = Guid.NewGuid();
			using (var command = Db.Connection.Command(CreateSupplierBookingHeaderSql))
			{
				command.AddParameter("@DH_PK", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@DH_OA_Consignor", SqlDbType.UniqueIdentifier, orgAddressPK);
				command.AddParameter("@DH_SystemCreateTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.AddParameter("@DH_SystemLastEditTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.ExecuteNonQuery();
			}

			return headerPK;
		}

		public void CreateSupplierBookingLine(Guid headerPK, Guid shipmentPK = default(Guid))
		{
			using (var command = Db.Connection.Command(CreateSupplierBookingLineSql))
			{
				command.AddParameter("@DL_PK", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("@DL_DH_BookingHeader", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@DL_SystemCreateTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.AddParameter("@DL_SystemLastEditTimeUtc", SqlDbType.DateTime, DateTime.Now);
				var shipmentPKOrNull = (shipmentPK == default(Guid)) ? (object)DBNull.Value : shipmentPK;
				command.AddParameter("@DL_JS_ApprovedShipment", SqlDbType.UniqueIdentifier, shipmentPKOrNull);
				command.ExecuteNonQuery();
			}
		}

		public Guid CreateCusInBondHeader(string appCode, Guid branchPK, Guid parentID, string parentTableCode, string voyageNunber, bool active = true)
		{
			using (DbCommand command = Db.Connection.Command(CreateCusInBondHeaderSql))
			{
				var pk = Guid.NewGuid();
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@appCode", SqlDbType.VarChar, CusInBondHeaderSchema.BH_ApplicationCode.MaxLength, appCode);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID == Guid.Empty ? DBNull.Value : parentID);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, CusInBondHeaderSchema.BH_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@voyageNumber", SqlDbType.VarChar, CusInBondHeaderSchema.BH_VoyageNumber.MaxLength, voyageNunber);
				command.AddParameter("@active", SqlDbType.Bit, active);
				command.ExecuteNonQuery();
				return pk;
			}
		}

		public Guid CreateUSInBondMoveHeader(Guid inBondHeaderPK)
		{
			using (DbCommand command = Db.Connection.Command(CreateUSInBondMoveHeaderSql))
			{
				var pk = Guid.NewGuid();
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@headerPK", SqlDbType.UniqueIdentifier, inBondHeaderPK == Guid.Empty ? DBNull.Value : inBondHeaderPK);
				command.ExecuteNonQuery();
				return pk;
			}
		}

		public Guid CreateCusInBondBill(Guid headerPK)
		{
			using (DbCommand command = Db.Connection.Command(CreateCusInBondBillSql))
			{
				var pk = Guid.NewGuid();
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@headerPK", SqlDbType.UniqueIdentifier, headerPK);
				command.ExecuteNonQuery();
				return pk;
			}
		}

		public Guid CreateCusInBondCargoDesc(Guid billPK)
		{
			using (DbCommand command = Db.Connection.Command(CreateCusInBondCargoDescSql))
			{
				var pk = Guid.NewGuid();
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@billPK", SqlDbType.UniqueIdentifier, billPK);
				command.ExecuteNonQuery();
				return pk;
			}
		}

		public Guid CreateCusInBondContainer(Guid parentID, string parentTableCode, string containerNumber, string seal1 = "", string seal2 = "")
		{
			using (DbCommand command = Db.Connection.Command(CreateCusInBondContainerSql))
			{
				var pk = Guid.NewGuid();
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID == Guid.Empty ? DBNull.Value : parentID);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, CusInBondContainerSchema.BC_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@containerNum", SqlDbType.VarChar, CusInBondContainerSchema.BC_ContainerNum.MaxLength, containerNumber);
				command.AddParameter("@seal1", SqlDbType.VarChar, CusInBondContainerSchema.BC_Seal1.MaxLength, seal1);
				command.AddParameter("@seal2", SqlDbType.VarChar, CusInBondContainerSchema.BC_Seal2.MaxLength, seal2);
				command.ExecuteNonQuery();
				return pk;
			}
		}
		public Guid CreateCusMAWB(Guid consolPK, bool active = true)
		{
			using (DbCommand command = Db.Connection.Command(CreateCusMAWBSql))
			{
				var pk = Guid.NewGuid();
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@consolPK", SqlDbType.UniqueIdentifier, consolPK);
				command.AddParameter("@active", SqlDbType.Bit, active);
				command.ExecuteNonQuery();
				return pk;
			}
		}

		public Guid CreateGenPivot(string relationType, string table1, Guid pk1, string table2, Guid pk2)
		{
			using (DbCommand command = Db.Connection.Command(CreateGenPivotSql))
			{
				var pk = Guid.NewGuid();
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@relationType", SqlDbType.VarChar, GenPivotSchema.XX_RelationType.MaxLength, relationType);
				command.AddParameter("@table1", SqlDbType.VarChar, GenPivotSchema.XX_Relation1TableCode.MaxLength, table1);
				command.AddParameter("@pk1", SqlDbType.UniqueIdentifier, pk1);
				command.AddParameter("@table2", SqlDbType.VarChar, GenPivotSchema.XX_Relation2TableCode.MaxLength, table2);
				command.AddParameter("@pk2", SqlDbType.UniqueIdentifier, pk2);
				command.ExecuteNonQuery();
				return pk;
			}
		}

		public Guid CreateOrgCountryData(Guid orgPK, Guid? addressPK = null, string approvedOrMajorExporter = "", string approvalNumber = "", DateTime? expiryDate = null, string countryCode = "", string issuingAuthorityCountryCode = "", string importCustomsDefaultAddInfo = "")
		{
			using (var command = Db.Connection.Command(CreateOrgCountryDataSql))
			{
				var pk = Guid.NewGuid();
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@orgPK", SqlDbType.UniqueIdentifier, orgPK);
				command.AddParameter("@addressPK", SqlDbType.UniqueIdentifier, addressPK == null ? DBNull.Value : addressPK);
				command.AddParameter("@approvedOrMajorExporter", SqlDbType.VarChar, OrgCountryDataSchema.OV_EXApprovedOrMajorExporter.MaxLength, approvedOrMajorExporter);
				command.AddParameter("@approvalNumber", SqlDbType.VarChar, OrgCountryDataSchema.OV_EXApprovalNumber.MaxLength, approvalNumber);
				command.AddParameter("@expiryDate", SqlDbType.DateTime, expiryDate == null ? DBNull.Value : expiryDate);
				command.AddParameter("@countryCode", SqlDbType.VarChar, OrgCountryDataSchema.OV_RN_NKClientCountryRelation.MaxLength, countryCode);
				command.AddParameter("@issuingAuthorityCountry", SqlDbType.VarChar, OrgCountryDataSchema.OV_RN_NKIssuingAuthorityCountry.MaxLength, issuingAuthorityCountryCode);
				command.AddParameter("@importCustomsDefaultAddInfo", SqlDbType.NVarChar, OrgCountryDataSchema.OV_ImportCustomsDefaultAddInfo.MaxLength, importCustomsDefaultAddInfo);
				command.ExecuteNonQuery();
				return pk;
			}
		}

		public Guid CreateCusEntryHeader(string dataModel, Guid declarationPK, int clusterKey, string messageType = "")
		{
			using (DbCommand command = Db.Connection.Command(CreateCusEntryHeaderSql))
			{
				var pk = Guid.NewGuid();
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel);
				command.AddParameter("@jobPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.ExecuteNonQuery();
				return pk;
			}
		}

		public Guid CreateCusEntryPayInfo(Guid chPK, int clusterKey)
		{
			using var command = Db.Connection.Command(CreateCusEntryPayInfoSql);
			var pk = Guid.NewGuid();
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@chPK", SqlDbType.UniqueIdentifier, chPK);
			command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
			command.ExecuteNonQuery();
			return pk;
		}

		public Guid CreateCusEntryLine(Guid entryHeaderPK, string dataModel, int clusterKey)
		{
			using (DbCommand command = Db.Connection.Command(CreateCusEntryLineSql))
			{
				var pk = Guid.NewGuid();
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@chPK", SqlDbType.UniqueIdentifier, entryHeaderPK);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
				return pk;
			}
		}

		public Guid CreateCusEntryLineFee(decimal chargeAmount, string chargeType, Guid entryLinePK, int clusterKey, string source)
		{
			using (DbCommand command = Db.Connection.Command(CreateCusEntryLineFeeSql))
			{
				var pk = Guid.NewGuid();
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@chargeAmount", SqlDbType.Money, chargeAmount);
				command.AddParameter("@chargeType", SqlDbType.Char, CusEntryLineFeeSchema.CF_ChargeType.MaxLength, chargeType);
				command.AddParameter("@clPK", SqlDbType.UniqueIdentifier, entryLinePK);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@source", SqlDbType.VarChar, source);
				command.ExecuteNonQuery();
				return pk;
			}
		}

		public Guid CreateSalesEnquiry(string uniqueReference, string companyName, string contactName, string enquiryType, string jobCategory)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateSalesEnquirySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@uniqueReference", SqlDbType.VarChar, OrgColdCallRegisterSchema.O1_LeadUniqueReference.MaxLength, uniqueReference);
				command.AddParameter("@companyName", SqlDbType.VarChar, OrgColdCallRegisterSchema.O1_CompanyName.MaxLength, companyName);
				command.AddParameter("@contactName", SqlDbType.VarChar, OrgColdCallRegisterSchema.O1_ContactName.MaxLength, contactName);
				command.AddParameter("@enquiryType", SqlDbType.VarChar, OrgColdCallRegisterSchema.O1_EnquiryType.MaxLength, enquiryType);
				command.AddParameter("@jobCategory", SqlDbType.VarChar, OrgColdCallRegisterSchema.O1_JobCategory.MaxLength, jobCategory);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public void CreateCusContainer(Guid cusContainerPK, Guid declarationPK, int clusterKey, string addInfo = "")
		{
			using (DbCommand command = Db.Connection.Command(CreateCusContainerSql))
			{
				command.AddParameter("@cusContainerPK", SqlDbType.UniqueIdentifier, cusContainerPK);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@addInfo", SqlDbType.NVarChar, addInfo);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}
		}

		public Guid CreateCusCodeData(string parentTableCode, Guid parentID, string type, string code, string data)
		{
			using var command = Db.Connection.Command(CreateCusCodeDataSql);
			var cusCodeDataPK = Guid.NewGuid();
			command.AddParameter("@cusCodeDataPK", SqlDbType.UniqueIdentifier, cusCodeDataPK);
			command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
			command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
			command.AddParameter("@type", SqlDbType.VarChar, type);
			command.AddParameter("@code", SqlDbType.VarChar, code);
			command.AddParameter("@data", SqlDbType.NVarChar, data);
			command.ExecuteNonQuery();
			return cusCodeDataPK;
		}

		#endregion

		#region Create SQl

		const string CreateCusEntryPayInfoSql = @"
			INSERT INTO dbo.CusEntryPayInfo (C9_PK, C9_CH, C9_ClusterKey, C9_SystemCreateTimeUtc, C9_SystemCreateUser, C9_SystemLastEditTimeUtc, C9_SystemLastEditUser)
			VALUES(@pk, @chPK, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateStorageMainSql = @"
			insert into dbo.StorageMain(SM_PK, SM_ParentFK, SM_Type, SM_DB, SM_SystemCreateTimeUtc, SM_SystemCreateUser, SM_SystemLastEditTimeUtc, SM_SystemLastEditUser)
			values(@pk, @parentPK, @type, @db, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateStorageDocSql = @"
			insert into {0}_SD{1:D3}..StorageDocs(SC_PK, SC_SM, SC_FileName, SC_Date, SC_ImageData, SC_DocType, SC_Desc, SC_SystemCreateTimeUtc, SC_SystemCreateUser)
			values(@pk, @parentPK, @fileName, @date, @data, @docType, @desc, @systemCreateTime, @lastEditTime)
			";

		const string CreateClientSql = @"
			insert into dbo.OrgHeader(OH_PK, OH_Code, OH_FullName, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser)
			values(@clientPK, @code, @name, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateAddressSql = @"
			insert into dbo.OrgAddress(OA_PK, OA_OH, OA_Code, OA_Address1, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser)
			values(@addressPK, @orgPK, @addressCode, @address1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateOrgAddressCapabilitySql = @"
			insert into dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress, PZ_SystemCreateTimeUtc, PZ_SystemCreateUser, PZ_SystemLastEditTimeUtc, PZ_SystemLastEditUser)
			values(@pzPK, @addressPK, @capabilityAddressType, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateNotMainOrgAddressCapabilitySql = @"
			insert into dbo.OrgAddressCapability(PZ_PK, PZ_OA, PZ_AddressType, PZ_IsMainAddress, PZ_SystemCreateTimeUtc, PZ_SystemCreateUser, PZ_SystemLastEditTimeUtc, PZ_SystemLastEditUser)
			values(@pzPK, @addressPK, @capabilityAddressType, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateOrgAppointedAgentPorts = @"
			INSERT INTO dbo.OrgAppointedAgentPorts
		   (O5_PK
		   ,O5_PortOrCountry
		   ,O5_SeaAgentStatus
		   ,O5_AirAgentStatus
		   ,O5_RailAgentStatus
		   ,O5_RoadAgentStatus
		   ,O5_OA_AgentOfficeAddress
		   ,O5_OH
		   ,O5_SeaAirCarrierOrForwarderType
		   ,O5_TerminalType
		   ,O5_ContainerType
		   ,O5_AgentDirection
		   ,O5_SystemCreateTimeUtc
		   ,O5_SystemCreateUser
		   ,O5_SystemLastEditTimeUtc
		   ,O5_SystemLastEditUser)
			VALUES
			(@O5_PK
			,@O5_PortOrCountry
			,@O5_SeaAgentStatus
			,@O5_AirAgentStatus
			,@O5_RailAgentStatus
			,@O5_RoadAgentStatus
			,@O5_OA_AgentOfficeAddress
			,@O5_OH
			,@O5_SeaAirCarrierOrForwarderType
			,@O5_TerminalType
			,@O5_ContainerType
			,@O5_AgentDirection
			,GetUtcDate()
			,'~BP'
			,GetUtcDate()
			,'~BP')";

		const string CreateOrgParkContainerTypeSql = @"
			INSERT INTO dbo.OrgParkContainerType(PT_PK, PT_ContainerStorageClass, PT_O5, PT_SystemCreateTimeUtc, PT_SystemCreateUser, PT_SystemLastEditTimeUtc, PT_SystemLastEditUser)
			VALUES(@PT_PK, @containerStorageClass, @agentPortPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateEquipmentSql = @"
			insert into dbo.RefEquipment(RQ_PK, RQ_ShortCode, RQ_SystemCreateTimeUtc, RQ_SystemCreateUser, RQ_SystemLastEditTimeUtc, RQ_SystemLastEditUser)
			values(@equipmentPK, @code, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateShipmentSql = @"INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_ShipmentType, JS_IsCancelled, JS_IsBooking, JS_IsHighRisk, JS_RL_NKOrigin, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser)
			VALUES (@shipmentPK, @shipmentNumber, @shipmentType, @cancelled, @isBooking, @isHighRisk, @origin, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateDeclarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DeclarationReference, JE_GB, JE_GC, JE_JS, JE_IsCancelled, JE_AddInfo, JE_MessageType, JE_ClusterKey, JE_DataModel, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser)
			VALUES (@declarationPK, @declarationNumber, @branchPK, @companyPK, @shipmentPK, @cancelled, @addInfo, @messageType, @clusterKey, @dataModel, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateContainerSql = @"INSERT INTO dbo.JobContainer (JC_PK, JC_ContainerNum, JC_JK, JC_RCA_AllocationLine, JC_SystemCreateTimeUtc, JC_SystemCreateUser, JC_SystemLastEditTimeUtc, JC_SystemLastEditUser)
			VALUES (@containerPK, @containerNum, @consolPK, @allocationLinePK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreatePackLineSql = @"INSERT INTO dbo.JobPackLines (JL_PK, JL_JS, JL_PackageCount, JL_ActualWeight, JL_ActualVolume, JL_SystemCreateTimeUtc, JL_SystemCreateUser, JL_SystemLastEditTimeUtc, JL_SystemLastEditUser)
			VALUES (@packLinePK, @shipmentPK, @packs, @weight, @volume, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateContainerLegSql = @"INSERT INTO dbo.JobContainerLegs (JU_PK, JU_Leg, JU_JJ, JU_E2PickupAddressID, JU_E2WaitPointAddressID, JU_E2DeliveryAddressID,
JU_TransportCoName, JU_DriversLicense, JU_DriversName, JU_TruckRegistration, JU_OH_TransportCo, JU_GS, JU_RQ_Vehicle, JU_DropMode, JU_DeliverySignedFor, JU_PickupTimeIn, JU_DeliverTimeIn, JU_GatePassNumber, JU_SplitDeliverySuffix, JU_SystemCreateTimeUtc, JU_SystemCreateUser, JU_SystemLastEditTimeUtc, JU_SystemLastEditUser)
			VALUES (@containerLegPK, @leg, @cartagePK, @pickupAddressPK, @waitPointAddressPK, @deliveryAddressPK, @transName, @licence, @driver, @rego, @transCoPK, @staffPK, @vehiclePK, @drop, @signed, @pickupTime, @deliverTime, @gatePassNumber, @splitDeliverySuffix, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateConsolSql = @"INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsCancelled, JK_RCA_AllocationLine, JK_OverrideWaybillDefaults, JK_RL_NKLoadPort, JK_RL_NKDischargePort, JK_TransportMode, JK_SystemCreateTimeUtc, JK_SystemCreateUser, JK_SystemLastEditTimeUtc, JK_SystemLastEditUser)
			VALUES (@consolPK, @consolNumber, @cancelled, @allocationLinePK, @overrideWaybillDefaults, @loadPort, @dischargePort, @transportMode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateJobConShipLinkSql = @"INSERT INTO dbo.JobConShipLink (JN_PK, JN_JK, JN_JS, JN_SystemCreateTimeUtc, JN_SystemCreateUser, JN_SystemLastEditTimeUtc, JN_SystemLastEditUser)
			VALUES (@linkPK, @consolPK, @shipmentPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateJobConsolTransportSql = @"INSERT INTO dbo.JobConsolTransport (JW_PK, JW_ParentGUID, JW_ParentType, JW_SystemCreateTimeUtc, JW_SystemCreateUser, JW_SystemLastEditTimeUtc, JW_SystemLastEditUser)
			VALUES (@jwPK, @parentGUID, @parentType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";

		const string CreateDocsAndCartageSql = @"INSERT INTO dbo.JobDocsAndCartage (JP_PK, JP_ParentTableCode, JP_ParentID, JP_DeliveryCartageCompleted, JP_SystemCreateTimeUtc, JP_SystemCreateUser, JP_SystemLastEditTimeUtc, JP_SystemLastEditUser)
			VALUES (@docsAndCartagePK, @parentTableCode, @parentID, @actualDeliveryTime, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateCompanySql = @"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_OH_OrgProxy, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
			VALUES (@companyPK, @companyCode, 'AU company', @countryCode, @currencyNK, @orgProxyPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateBranchSql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC, GB_OH_OrgProxy, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
			VALUES (@branchPK, @branchCode, @homePort, @CompanyPK, @OrgProxy, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
		const string CreateJobHeaderSql = @"INSERT INTO dbo.JobHeader (JH_PK, JH_ParentID, JH_ParentTableCode, JH_JobNum, JH_GC, JH_GB, JH_GE, JH_JH_ParentJob, JH_Status, JH_ClientContractNumber, JH_SystemCreateTimeUtc, JH_SystemCreateUser, JH_SystemLastEditTimeUtc, JH_SystemLastEditUser)
			VALUES (@jobHeaderPK, @parentPK, @parentTableCode, @jobNum, @companyPK, @branchPK, @departmentPK, @parentJobPK, 'WRK', @clientContractNum, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
		const string CreateJobChargeSql = @"INSERT INTO dbo.JobCharge (JR_PK, JR_JH, JR_AC, JR_GB, JR_GC, JR_GE, JR_SystemCreateTimeUtc, JR_SystemCreateUser, JR_SystemLastEditTimeUtc, JR_SystemLastEditUser)
			VALUES (@jobChargePK, @jobHeaderPK, @chargeCodePK, @branchPK, @companyPK, @departmentPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateChargeCodeSql = @"INSERT INTO dbo.AccChargeCode (AC_PK, AC_Code, AC_ChargeType, AC_AG_RevenueAccount, AC_AG_WIPAccount, AC_AG_CostAccount, AC_AG_AccrualAccount, AC_GC, AC_AT_GSTRate, AC_SystemCreateTimeUtc, AC_SystemCreateUser, AC_SystemLastEditTimeUtc, AC_SystemLastEditUser)
			VALUES (@PK, @Code, @ChargeType, @RevenueAccount, @WIPAccount, @CostAccount, @AccrualAccount, @Company, @GSTRate, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateDepartmentSql = @"INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code, GE_SystemCreateTimeUtc, GE_SystemCreateUser, GE_SystemLastEditTimeUtc, GE_SystemLastEditUser)
			VALUES (@departmentPK, @departmentCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateStmALogSql = @"INSERT INTO dbo.StmALog (SL_PK, SL_Table, SL_Parent, SL_GS_NKUser, SL_SE_NKEvent, SL_EventTime, SL_PostedTimeUtc, SL_Reference, SL_IsCancelled)
			VALUES (@pk, @tableName, @parentPK, @userCode, @eventCode, @date, @utcDate, @reference, @cancelled)";

		const string CreateStmJobQueueSql = @"INSERT INTO dbo.StmJobQueue (SJ_PK, SJ_PostedTimeUtc, SJ_FilterName, SJ_Status, SJ_ParentID, SJ_ALogReference, SJ_EventTime, SJ_EventTimeUtc, SJ_ParentTableCode)
			VALUES (@pk, @postedTimeUtc, @filterName, @status, @parentID, @logReference, @eventTime, @eventTimeUtc, @parentTableCode)";

		const string CreateStaffSql = @"INSERT INTO dbo.GlbStaff (GS_PK, GS_LoginName, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
			VALUES (@pk, @loginName, @code, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateHRJobApplicantSql = @"INSERT INTO dbo.HRJobApplicant (HA_PK, HA_EmailAddress, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser)
			VALUES (@pk, @emailAddress, @personPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreatePersonSql = @"INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser)
			VALUES(@pk, @fullName, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateOrgStaffAssignmentsSql = @"INSERT INTO dbo.OrgStaffAssignments (O8_PK, O8_Role, O8_Department, O8_OH, O8_GS_NKPersonResponsible, O8_SystemCreateTimeUtc, O8_SystemCreateUser, O8_SystemLastEditTimeUtc, O8_SystemLastEditUser)
			VALUES (@pk, @role, @department, @orgHeaderPk, @personResponsible, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateOrgSql = @"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_IsConsignor, OH_IsForwarder, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser)
			VALUES (@pk, @code, @fullName, @isConsignor, @isForwarder, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateOrgSecuritySql = @"INSERT INTO dbo.OrgSecurity (OX_PK, OX_SecurityItemName, OX_OH, OX_Granted, OX_SU, OX_SystemCreateTimeUtc, OX_SystemCreateUser, OX_SystemLastEditTimeUtc, OX_SystemLastEditUser)
			VALUES (@pk, @itemName, @orgPK, @isGranted, @stmMenuItemPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateOrgSecurityContactsSql = @"INSERT INTO dbo.OrgSecurityContacts (OZ_PK, OZ_OC, OZ_OX, OZ_Granted, OZ_SystemCreateTimeUtc, OZ_SystemCreateUser, OZ_SystemLastEditTimeUtc, OZ_SystemLastEditUser)
			VALUES (@pk, @contactPK, @securityPK, @isGranted, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateContactSql = @"INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_PER, OC_Email, OC_SystemCreateTimeUtc, OC_SystemCreateUser, OC_SystemLastEditTimeUtc, OC_SystemLastEditUser)
			VALUES (@pk, @name, @orgPk, @personPk, @email, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateContactWithJobCategorySql = @"INSERT INTO dbo.OrgContact (OC_PK, OC_ContactName, OC_OH, OC_JobCategory, OC_SystemCreateTimeUtc, OC_SystemCreateUser, OC_SystemLastEditTimeUtc, OC_SystemLastEditUser)
			VALUES (@pk, @name, @orgPk, @jobCategory, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateDocAddressSql = @"INSERT INTO dbo.JobDocAddress(E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_AddressOverride, E2_ValidationStatus, E2_CompanyName, E2_AddressSequence, E2_OA_Address, E2_GovRegNumType, E2_GovRegNum, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
				VALUES(@PK, @parentPK, @parentTableCode, @AddressType, @AddressOverride, @ValidationStatus, @CompanyName, @addressSeq, @addressPK, @govRegNumTyp, @govRegNum, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateSpotQuoteHeaderSql = @"INSERT INTO dbo.RatingHeader(TH_PK, TH_RateType, TH_OneTimeQuote, TH_QuoteNumber, TH_QuoteDate, TH_SystemCreateTimeUtc, TH_SystemCreateUser, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser)
			VALUES(@pk, 'QTE', 1, @quoteNumber, @quoteDateTime, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateOneOffShipmentSql = @"insert into dbo.RateOneOffShipment (TT_PK, TT_TH, TT_SystemCreateTimeUtc, TT_SystemCreateUser, TT_SystemLastEditTimeUtc, TT_SystemLastEditUser)
			VALUES(@pk, @ratingHeaderPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateOneOffContainersSql = @"insert into dbo.RateOneOffContainers (TC_PK, TC_TT, TC_F3_NKPackType, TC_UnitOfDimension, TC_UnitOfVolume, TC_UnitOfWeight, TC_SystemCreateTimeUtc, TC_SystemCreateUser, TC_SystemLastEditTimeUtc, TC_SystemLastEditUser)
			VALUES(@pk, @oneOffShipmentPK, @packType, @unitOfDimension,  @unitOfVolume, @unitOfWeight, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateRateTariffDiscountSql = @"insert into dbo.RateTariffDiscount (TD_PK, TD_TH, TD_SystemCreateTimeUtc, TD_SystemCreateUser, TD_SystemLastEditTimeUtc, TD_SystemLastEditUser)
			VALUES(@pk, @ratingHeaderPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateRateAttachmentSql = @"insert into dbo.RateAttachment (TA_PK, TA_TH, TA_TS, TA_SystemCreateTimeUtc, TA_SystemCreateUser, TA_SystemLastEditTimeUtc, TA_SystemLastEditUser)
			VALUES(@pk, @ratingHeaderPK, @rateAttachmentSetPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateRateAttachmentSetSql = @"insert into dbo.RateAttachmentSet (TS_PK, TS_SystemCreateTimeUtc, TS_SystemCreateUser, TS_SystemLastEditTimeUtc, TS_SystemLastEditUser)
			VALUES(@pk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateOrgCusCodeSql = @"
INSERT dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_OH, OK_RN_NKCodeCountry, OK_SystemCreateTimeUtc, OK_SystemCreateUser, OK_SystemLastEditTimeUtc, OK_SystemLastEditUser)
VALUES(@Pk, @CustomsRegNo, @Type, @OrgPk, @CountryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateOrgCusCodeWithPremiseSql = @"
INSERT dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_OH, OK_RN_NKCodeCountry, OK_OA_PremisesAddress, OK_SystemCreateTimeUtc, OK_SystemCreateUser, OK_SystemLastEditTimeUtc, OK_SystemLastEditUser)
VALUES(@Pk, @CustomsRegNo, @Type, @OrgPk, @CountryCode, @addressPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateSupplierBookingHeaderSql = @"
INSERT INTO dbo.SupplierBookingHeader (DH_PK, DH_OA_Consignor, DH_SystemCreateTimeUtc, DH_SystemCreateUser, DH_SystemLastEditTimeUtc, DH_SystemLastEditUser)
VALUES (@DH_PK, @DH_OA_Consignor, @DH_SystemCreateTimeUtc, '~BP', @DH_SystemLastEditTimeUtc, '~BP')";

		const string CreateSupplierBookingLineSql = @"
INSERT INTO dbo.SupplierBookingLine (DL_PK, DL_DH_BookingHeader, DL_SystemCreateTimeUtc, DL_SystemCreateUser, DL_SystemLastEditTimeUtc, DL_SystemLastEditUser, DL_JS_ApprovedShipment)
VALUES (@DL_PK, @DL_DH_BookingHeader, @DL_SystemCreateTimeUtc, '~BP', @DL_SystemLastEditTimeUtc, '~BP', @DL_JS_ApprovedShipment)";

		const string CreateCusInBondHeaderSql = @"
INSERT INTO dbo.CusInBondHeader(BH_PK, BH_ApplicationCode, BH_GB, BH_ParentID, BH_ParentTableCode, BH_VoyageNumber, BH_IsActive, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser)
VALUES (@pk, @appCode, @branchPK, @parentID, @parentTableCode, @voyageNumber, @active, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateUSInBondMoveHeaderSql = @"
INSERT INTO dbo.USInBondMoveHeader(BMH_PK, BMH_BH, BMH_SystemCreateTimeUtc, BMH_SystemCreateUser, BMH_SystemLastEditTimeUtc, BMH_SystemLastEditUser)
VALUES (@pk, @headerPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateCusInBondBillSql = @"
INSERT INTO dbo.CusInBondBill(B0_PK, B0_BH, B0_SystemCreateTimeUtc, B0_SystemCreateUser, B0_SystemLastEditTimeUtc, B0_SystemLastEditUser)
VALUES (@pk, @headerPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateCusInBondCargoDescSql = @"
INSERT INTO dbo.CusInBondCargoDesc(BY_PK, BY_ParentID, BY_ParentTableCode, BY_SystemCreateTimeUtc, BY_SystemCreateUser, BY_SystemLastEditTimeUtc, BY_SystemLastEditUser)
VALUES (@pk, @billPK, 'B0', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateCusInBondContainerSql = @"
INSERT INTO dbo.CusInBondContainer(BC_PK, BC_ParentID, BC_ParentTableCode, BC_ContainerNum, BC_Seal1, BC_Seal2, BC_SystemCreateTimeUtc, BC_SystemCreateUser, BC_SystemLastEditTimeUtc, BC_SystemLastEditUser)
VALUES (@pk, @parentID, @parentTableCode, @containerNum, @seal1, @seal2, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateCusMAWBSql = @"
INSERT INTO dbo.CusMAWB(CM_PK, CM_JK, CM_IsActive)
VALUES (@pk, @consolPK, @active)";

		const string CreateGenPivotSql = @"
			insert into dbo.GenPivot (XX_PK, XX_RelationType, XX_Relation1TableCode, XX_Relation1ID, XX_Relation2TableCode, XX_Relation2ID, XX_SystemCreateTimeUtc, XX_SystemCreateUser, XX_SystemLastEditTimeUtc, XX_SystemLastEditUser)
			values(@pk, @relationType, @table1, @pk1, @table2, @pk2, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateOrgCountryDataSql = @"
INSERT INTO dbo.OrgCountryData (OV_PK, OV_OH_OrgHeader, OV_OA_ApprovedLocation, OV_EXApprovedOrMajorExporter, OV_EXApprovalNumber, OV_EXApprovalExpiryDate, OV_ImportCustomsDefaultAddInfo, OV_RN_NKClientCountryRelation, OV_RN_NKIssuingAuthorityCountry, OV_SystemCreateTimeUtc, OV_SystemCreateUser, OV_SystemLastEditTimeUtc, OV_SystemLastEditUser)
VALUES (@pk, @orgPK, @addressPK, @approvedOrMajorExporter, @approvalNumber, @expiryDate, @importCustomsDefaultAddInfo, @countryCode, @issuingAuthorityCountry, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateCusEntryHeaderSql = @"INSERT INTO dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_ClusterKey, CH_MessageType, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) VALUES(@pk, @dataModel, @jobPK, @clusterKey, @messageType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateCusEntryLineSql = @"INSERT INTO dbo.CusEntryLine (CL_PK, CL_CH, CL_DataModel, CL_ClusterKey, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser) VALUES(@pk, @chPK, @dataModel, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateCusEntryLineFeeSql = @"
INSERT INTO dbo.CusEntryLineFee (CF_PK, CF_ChargeAmount, CF_ChargeType, CF_CL, CF_ClusterKey, CF_Source, CF_SystemCreateTimeUtc, CF_SystemCreateUser, CF_SystemLastEditTimeUtc, CF_SystemLastEditUser)
VALUES(@pk, @chargeAmount, @chargeType, @clPK, @clusterKey, @source, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateSalesEnquirySql = @"
INSERT INTO dbo.OrgColdCallRegister(O1_PK, O1_LeadUniqueReference, O1_CompanyName, O1_ContactName, O1_EnquiryType, O1_JobCategory, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser)
VALUES(@pk, @uniqueReference, @companyName, @contactName, @enquiryType, @jobCategory, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateCusContainerSql = @"INSERT INTO dbo.CusContainer (CO_PK, CO_JE, CO_AddInfo, CO_ClusterKey, CO_SystemCreateTimeUtc, CO_SystemCreateUser, CO_SystemLastEditTimeUtc, CO_SystemLastEditUser)
			VALUES (@cusContainerPK, @declarationPK, @addInfo, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateCusCodeDataSql = @"INSERT INTO dbo.CusCodeData (CY_PK, CY_ParentTableCode, CY_ParentID, CY_Type, CY_Code, CY_Data, CY_SystemCreateTimeUtc, CY_SystemCreateUser, CY_SystemLastEditTimeUtc, CY_SystemLastEditUser)
			VALUES (@cusCodeDataPK, @parentTableCode, @parentID, @type, @code, @data, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateStmLinkSql = "INSERT INTO dbo.StmLink(STL_PK, STL_ModuleID, STL_LastUsedDateTimeUtc, STL_SystemLastEditUser, STL_GC_LogonCompany , STL_LinkType, STL_GS_NKUser) Values(@pk,@moduleID, GetUtcDate(), '~BP', @logonCompany, 'RUM','~BP')";

		const string CreateCusEntryInstructionSql = @"INSERT INTO dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_ClusterKey, CEI_JE, CEI_Style, CEI_SubStyle, CEI_MergeBy, CEI_AddInfo, CEI_Description, CEI_Procedure, CEI_DisplaySequence, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser) VALUES (@CeiPK, @CeiDataModel, @CeiClusterKey, @DeclarationPK, @CeiStyle, @CeiSubstyle, @CeiMergeBy, @CeiAddInfo, @CeiDescription, @CeiProcedure, @CeiDisplaySequence, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateOrgTimetableSql = "INSERT INTO dbo.OrgTimetable([OTT_PK], [OTT_OA], [OTT_Type], [OTT_Monday], [OTT_Tuesday], [OTT_Wednesday], [OTT_Thursday], [OTT_Friday], [OTT_Saturday], [OTT_Sunday], [OTT_TimeFrom], [OTT_TimeTo], [OTT_SystemCreateTimeUtc], [OTT_SystemCreateUser], [OTT_SystemLastEditTimeUtc], [OTT_SystemLastEditUser]) VALUES(@pk, @orgAddressPk, @type, @monday, @tuesday, @wednesday, @thursday, @friday, @saturday, @sunday, @timeFrom, @timeTo, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateOrgTimetableWithEmptyAuditInfoSql = @"
			IF EXISTS(SELECT null FROM sys.objects WHERE name = 'TG_OrgTimetable_AuditDetailsAreNotMissing_Insert')
			BEGIN
				DISABLE TRIGGER OrgTimetable.TG_OrgTimetable_AuditDetailsAreNotMissing_Insert ON dbo.OrgTimetable;
			END
			INSERT INTO dbo.OrgTimetable([OTT_PK], [OTT_OA], [OTT_Type], [OTT_Monday], [OTT_Tuesday], [OTT_Wednesday], [OTT_Thursday], [OTT_Friday], [OTT_Saturday], [OTT_Sunday], [OTT_TimeFrom], [OTT_TimeTo]) VALUES(@pk, @orgAddressPk, @type, @monday, @tuesday, @wednesday, @thursday, @friday, @saturday, @sunday, @timeFrom, @timeTo);
			IF EXISTS(SELECT null FROM sys.objects WHERE name = 'TG_OrgTimetable_AuditDetailsAreNotMissing_Insert')
			BEGIN
			ENABLE TRIGGER OrgTimetable.TG_OrgTimetable_AuditDetailsAreNotMissing_Insert ON dbo.OrgTimetable;
			END";

		const string CreateStmDataSql = @"INSERT INTO [dbo].[StmData] ([SD_PK], [SD_Name], [SD_Owner], [SD_Type]) VALUES (@pk, 'ExcelPassword', newid(), 'STR')";
		#endregion

		#region CreateAccBankAccount

		const string CreateAccBankAccountSql = @"INSERT INTO dbo.AccBankAccount(AB_PK, AB_Code, AB_LastReconcileDate, AB_LastStatementDate, AB_GC, AB_AG, AB_SystemCreateTimeUtc, AB_SystemCreateUser, AB_SystemLastEditTimeUtc, AB_SystemLastEditUser)
VALUES (@pk, @code, @lastReconcileDate, @lastStatementDate, @gc, @ag, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateAccBankAccount(string code, Guid glbCompany, Guid accGLHeader)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccBankAccountSql))
			{
				command.AddParameter("@code", SqlDbType.VarChar, AccBankAccountSchema.AB_Code.MaxLength, code);
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@lastReconcileDate", SqlDbType.SmallDateTime, DateTime.Now);
				command.AddParameter("@lastStatementDate", SqlDbType.SmallDateTime, DateTime.Now);
				command.AddParameter("@gc", SqlDbType.UniqueIdentifier, glbCompany);
				command.AddParameter("@ag", SqlDbType.UniqueIdentifier, accGLHeader);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateAccChargeCode

		const string CreateAccChargeCodeSql = @"INSERT INTO dbo.AccChargeCode(AC_PK, AC_Code, AC_Desc, AC_ChargeType, AC_GC, AC_SystemCreateTimeUtc, AC_SystemCreateUser, AC_SystemLastEditTimeUtc, AC_SystemLastEditUser) VALUES (@pk, @code, @desc, @chargeType, @gc, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateAccChargeCode(string code, string chargeType, Guid glbCompany)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccChargeCodeSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, AccChargeCodeSchema.AC_Code.MaxLength, code);
				command.AddParameter("@desc", SqlDbType.VarChar, AccChargeCodeSchema.AC_Desc.MaxLength, "Test Charge Code");
				command.AddParameter("@chargeType", SqlDbType.Char, AccChargeCodeSchema.AC_ChargeType.MaxLength, chargeType);
				command.AddParameter("@gc", SqlDbType.UniqueIdentifier, glbCompany);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateAccCollectionBatch

		const string CreateAccCollectionBatchSql = @"INSERT INTO dbo.AccCollectionBatch (ACB_PK, ACB_Type, ACB_BatchNumber, ACB_TotalAmount, ACB_AB, ACB_GC, ACB_SystemCreateTimeUtc, ACB_SystemCreateUser, ACB_SystemLastEditTimeUtc, ACB_SystemLastEditUser)
VALUES (@pk, @type, @batchNumber, @amount, @ab, @gc, @createTime, @createUser, @editTime, @editUser)";

		public Guid CreateAccCollectionBatch(string batchNumber, int amount, Guid accBankAccount, Guid glbCompany)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccCollectionBatchSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@type", SqlDbType.Char, AccCollectionBatchSchema.ACB_Type.MaxLength, "STD");
				command.AddParameter("@batchNumber", SqlDbType.VarChar, AccGLHeaderSchema.AG_AccountGroup.MaxLength, batchNumber);
				command.AddParameter("@amount", SqlDbType.Money, amount);
				command.AddParameter("@ab", SqlDbType.UniqueIdentifier, accBankAccount);
				command.AddParameter("@gc", SqlDbType.UniqueIdentifier, glbCompany);
				command.AddParameter("@createTime", SqlDbType.SmallDateTime, DateTime.Now);
				command.AddParameter("@createUser", SqlDbType.VarChar, AccCollectionBatchSchema.ACB_SystemCreateUser.MaxLength, "AAA");
				command.AddParameter("@editTime", SqlDbType.SmallDateTime, DateTime.Now);
				command.AddParameter("@editUser", SqlDbType.VarChar, AccCollectionBatchSchema.ACB_SystemLastEditUser.MaxLength, "AAA");
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateAccCommissionApprovalRequest

		const string CreateAccCommissionApprovalRequestSql = @"INSERT INTO dbo.AccCommissionApprovalRequest (CRQ_PK, CRQ_BatchNumber, CRQ_GS_NKApprovingStaff1, CRQ_GS_NKApprovingStaff2, CRQ_Staff1HasApproved, CRQ_Staff2HasApproved, CRQ_SystemCreateTimeUtc, CRQ_SystemCreateUser, CRQ_SystemLastEditTimeUtc, CRQ_SystemLastEditUser)
VALUES (@pk, @batchNumber, @approvingStaffCode1, @approvingStaffCode2, @staff1HasApproved, @staff2HasApproved, @createTime, @createUser, @editTime, @editUser)";

		public Guid CreateAccCommissionApprovalRequest(string batchNumber, string approvingStaffCode1, string approvingStaffCode2, bool staff1HasApproved, bool staff2HasApproved)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccCommissionApprovalRequestSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, AccCommissionApprovalRequestSchema.PK);
				command.AddParameterBasedOnDbColumn("@batchNumber", batchNumber, AccCommissionApprovalRequestSchema.CRQ_BatchNumber);
				command.AddParameterBasedOnDbColumn("@approvingStaffCode1", approvingStaffCode1, AccCommissionApprovalRequestSchema.CRQ_GS_NKApprovingStaff1);
				command.AddParameterBasedOnDbColumn("@approvingStaffCode2", approvingStaffCode2, AccCommissionApprovalRequestSchema.CRQ_GS_NKApprovingStaff2);
				command.AddParameterBasedOnDbColumn("@staff1HasApproved", staff1HasApproved, AccCommissionApprovalRequestSchema.CRQ_Staff1HasApproved);
				command.AddParameterBasedOnDbColumn("@staff2HasApproved", staff2HasApproved, AccCommissionApprovalRequestSchema.CRQ_Staff2HasApproved);
				command.AddParameterBasedOnDbColumn("@createTime", DateTime.UtcNow, AccCommissionApprovalRequestSchema.CRQ_SystemCreateTimeUtc);
				command.AddParameterBasedOnDbColumn("@createUser", "AAA", AccCommissionApprovalRequestSchema.CRQ_SystemCreateUser);
				command.AddParameterBasedOnDbColumn("@editTime", DateTime.UtcNow, AccCommissionApprovalRequestSchema.CRQ_SystemLastEditTimeUtc);
				command.AddParameterBasedOnDbColumn("@editUser", "AAA", AccCommissionApprovalRequestSchema.CRQ_SystemLastEditUser);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateAccCommissionApprovalRequestItem

		const string CreateAccCommissionApprovalRequestItemSql = @"INSERT INTO dbo.AccCommissionApprovalRequestItem (CRI_PK, CRI_CRQ, CRI_CL0, CRI_IsSelected, CRI_SystemCreateTimeUtc, CRI_SystemCreateUser, CRI_SystemLastEditTimeUtc, CRI_SystemLastEditUser)
VALUES (@pk, @requestPk, @commissionLinePk, @isSelected, @createTime, @createUser, @editTime, @editUser)";

		public Guid CreateAccCommissionApprovalRequestItem(Guid requestPk, Guid commissionLinePk, bool isSelected)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccCommissionApprovalRequestItemSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, AccCommissionApprovalRequestItemSchema.PK);
				command.AddParameterBasedOnDbColumn("@requestPk", requestPk, AccCommissionApprovalRequestItemSchema.CRI_CRQ);
				command.AddParameterBasedOnDbColumn("@commissionLinePk", commissionLinePk, AccCommissionApprovalRequestItemSchema.CRI_CL0);
				command.AddParameterBasedOnDbColumn("@isSelected", isSelected, AccCommissionApprovalRequestItemSchema.CRI_IsSelected);
				command.AddParameterBasedOnDbColumn("@createTime", DateTime.UtcNow, AccCommissionApprovalRequestItemSchema.CRI_SystemCreateTimeUtc);
				command.AddParameterBasedOnDbColumn("@createUser", "AAA", AccCommissionApprovalRequestItemSchema.CRI_SystemCreateUser);
				command.AddParameterBasedOnDbColumn("@editTime", DateTime.UtcNow, AccCommissionApprovalRequestItemSchema.CRI_SystemLastEditTimeUtc);
				command.AddParameterBasedOnDbColumn("@editUser", "AAA", AccCommissionApprovalRequestItemSchema.CRI_SystemLastEditUser);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateAccCommissionHeader

		const string CreateAccCommissionHeaderSql = @"INSERT INTO dbo.AccCommissionHeader (CH0_PK, CH0_GC, CH0_AH_Source, CH0_GroupingSourceID, CH0_GroupingSourceTableCode, CH0_CA0, CH0_OH_Customer, CH0_Product, CH0_Service, CH0_SubModule, CH0_CommissionDate, CH0_SnapshotDateTime, CH0_SnapshotEventCode, CH0_SystemCreateTimeUtc, CH0_SystemCreateUser, CH0_Mode, CH0_NKDestination, CH0_NKOrigin, CH0_SystemLastEditTimeUtc, CH0_SystemLastEditUser)
VALUES (@pk, @companyPk, @invoicePk, @groupingSourceId, @groupingSourceTableCode, @agreementPk, @customerPk, @product, @service, @subModule, @commissionDate, @snapshotDateTime, @snapshotEventCode, @createTime, @createUser, @mode, @destination, @origin, @editTime, @editUser)";

		public Guid CreateAccCommissionHeader(Guid companyPk, Guid invoicePk, Guid groupingSourceId, string groupingSourceTableCode, Guid agreementPk, Guid customerPk, string product, string service, string subModule, DateTime commissionDate, string mode, string destination, string origin)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccCommissionHeaderSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, AccCommissionHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@companyPk", companyPk, AccCommissionHeaderSchema.CH0_GC);
				command.AddParameterBasedOnDbColumn("@invoicePk", invoicePk, AccCommissionHeaderSchema.CH0_AH_Source);
				command.AddParameterBasedOnDbColumn("@groupingSourceId", groupingSourceId == Guid.Empty ? DBNull.Value : groupingSourceId, AccCommissionHeaderSchema.CH0_GroupingSourceID);
				command.AddParameterBasedOnDbColumn("@groupingSourceTableCode", groupingSourceTableCode, AccCommissionHeaderSchema.CH0_GroupingSourceTableCode);
				command.AddParameterBasedOnDbColumn("@agreementPk", agreementPk, AccCommissionHeaderSchema.CH0_CA0);
				command.AddParameterBasedOnDbColumn("@customerPk", customerPk, AccCommissionHeaderSchema.CH0_OH_Customer);
				command.AddParameterBasedOnDbColumn("@product", product, AccCommissionHeaderSchema.CH0_Product);
				command.AddParameterBasedOnDbColumn("@service", service, AccCommissionHeaderSchema.CH0_Service);
				command.AddParameterBasedOnDbColumn("@subModule", subModule, AccCommissionHeaderSchema.CH0_SubModule);
				command.AddParameterBasedOnDbColumn("@commissionDate", commissionDate, AccCommissionHeaderSchema.CH0_CommissionDate);
				command.AddParameterBasedOnDbColumn("@snapshotDateTime", DateTime.Now, AccCommissionHeaderSchema.CH0_SnapshotDateTime);
				command.AddParameterBasedOnDbColumn("@snapshotEventCode", "JCL", AccCommissionHeaderSchema.CH0_SnapshotEventCode);
				command.AddParameterBasedOnDbColumn("@createTime", DateTime.UtcNow, AccCommissionHeaderSchema.CH0_SystemCreateTimeUtc);
				command.AddParameterBasedOnDbColumn("@createUser", "AAA", AccCommissionHeaderSchema.CH0_SystemCreateUser);
				command.AddParameterBasedOnDbColumn("@mode", mode, AccCommissionHeaderSchema.CH0_Mode);
				command.AddParameterBasedOnDbColumn("@destination", destination, AccCommissionHeaderSchema.CH0_NKDestination);
				command.AddParameterBasedOnDbColumn("@origin", origin, AccCommissionHeaderSchema.CH0_NKOrigin);
				command.AddParameterBasedOnDbColumn("@editTime", DateTime.UtcNow, AccCommissionHeaderSchema.CH0_SystemLastEditTimeUtc);
				command.AddParameterBasedOnDbColumn("@editUser", "AAA", AccCommissionHeaderSchema.CH0_SystemLastEditUser);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateAccCommissionLine

		const string CreateAccCommissionLineSql = @"INSERT INTO dbo.AccCommissionLine (CL0_PK, CL0_ParentID, CL0_ParentTableCode, CL0_GS_NKStaff, CL0_RX_NKTransactionCurrency, CL0_TransactionAmount, CL0_CommissionType, CL0_RX_NKCommissionCurrency, CL0_TotalCommissionableAmount, CL0_ShareTotal, CL0_SharePortion, CL0_ShareCommissionAmount, CL0_EntityCommissionAmount, CL0_SystemCreateTimeUtc, CL0_SystemCreateUser, CL0_EntityPercentage, CL0_ShouldReinstate, CL0_SystemLastEditTimeUtc, CL0_SystemLastEditUser)
VALUES (@pk, @parentId, @parentTableCode, @staffCode, @transactionCurrency, @transactionAmount, @commissionType, @commissionCurrency, @totalCommissionableAmount, @shareTotal, @sharePortion, @shareCommissionAmount, @entityCommissionAmount, @createTime, @createUser, @entityPercentage, @shouldReinstate, @editTime, @editUser)";

		public Guid CreateAccCommissionLine(Guid parentId, string parentTableCode, string staffCode, string transactionCurrency, decimal transactionAmount, string commissionType, string commissionCurrency, decimal totalCommissionableAmount, int shareTotal, int sharePortion, decimal shareCommissionAmount, decimal entityCommissionAmount, decimal entityPercentage, bool shouldReinstate)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccCommissionLineSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, AccCommissionLineSchema.PK);
				command.AddParameterBasedOnDbColumn("@parentId", parentId == Guid.Empty ? DBNull.Value : parentId, AccCommissionLineSchema.CL0_ParentID);
				command.AddParameterBasedOnDbColumn("@parentTableCode", parentTableCode, AccCommissionLineSchema.CL0_ParentTableCode);
				command.AddParameterBasedOnDbColumn("@staffCode", staffCode, AccCommissionLineSchema.CL0_GS_NKStaff);
				command.AddParameterBasedOnDbColumn("@transactionCurrency", transactionCurrency, AccCommissionLineSchema.CL0_RX_NKTransactionCurrency);
				command.AddParameterBasedOnDbColumn("@transactionAmount", transactionAmount, AccCommissionLineSchema.CL0_TransactionAmount);
				command.AddParameterBasedOnDbColumn("@commissionType", commissionType, AccCommissionLineSchema.CL0_CommissionType);
				command.AddParameterBasedOnDbColumn("@commissionCurrency", commissionCurrency, AccCommissionLineSchema.CL0_RX_NKCommissionCurrency);
				command.AddParameterBasedOnDbColumn("@totalCommissionableAmount", totalCommissionableAmount, AccCommissionLineSchema.CL0_TotalCommissionableAmount);
				command.AddParameterBasedOnDbColumn("@shareTotal", shareTotal, AccCommissionLineSchema.CL0_ShareTotal);
				command.AddParameterBasedOnDbColumn("@sharePortion", sharePortion, AccCommissionLineSchema.CL0_SharePortion);
				command.AddParameterBasedOnDbColumn("@shareCommissionAmount", shareCommissionAmount, AccCommissionLineSchema.CL0_ShareCommissionAmount);
				command.AddParameterBasedOnDbColumn("@entityCommissionAmount", entityCommissionAmount, AccCommissionLineSchema.CL0_EntityCommissionAmount);
				command.AddParameterBasedOnDbColumn("@createTime", DateTime.UtcNow, AccCommissionLineSchema.CL0_SystemCreateTimeUtc);
				command.AddParameterBasedOnDbColumn("@createUser", "AAA", AccCommissionLineSchema.CL0_SystemCreateUser);
				command.AddParameterBasedOnDbColumn("@entityPercentage", entityPercentage, AccCommissionLineSchema.CL0_EntityPercentage);
				command.AddParameterBasedOnDbColumn("@shouldReinstate", shouldReinstate, AccCommissionLineSchema.CL0_ShouldReinstate);
				command.AddParameterBasedOnDbColumn("@editTime", DateTime.UtcNow, AccCommissionLineSchema.CL0_SystemLastEditTimeUtc);
				command.AddParameterBasedOnDbColumn("@editUser", "AAA", AccCommissionLineSchema.CL0_SystemLastEditUser);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateAccCommissionLineGroup

		const string CreateAccCommissionLineGroupSql = @"INSERT INTO dbo.AccCommissionLineGroup (CLG_PK, CLG_CH0, CLG_AC, CLG_TransactionAmount, CLG_RX_NKTransactionCurrency, CLG_TotalCommissionableAmount, CLG_RX_NKCommissionCurrency, CLG_CommissionDate, CLG_SystemCreateTimeUtc, CLG_SystemCreateUser, CLG_SystemLastEditTimeUtc, CLG_SystemLastEditUser)
VALUES (@pk, @headerPk, @chargePk, @transactionAmount, @transactionCurrency, @totalCommissionableAmount, @commissionCurrency, @commissionDate, @createTime, @createUser, @editTime, @editUser)";

		public Guid CreateAccCommissionLineGroup(Guid headerPk, Guid chargePk, decimal transactionAmount, string transactionCurrency, decimal totalCommissionableAmount, string commissionCurrency, DateTime commissionDate)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccCommissionLineGroupSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, AccCommissionLineGroupSchema.PK);
				command.AddParameterBasedOnDbColumn("@headerPk", headerPk, AccCommissionLineGroupSchema.CLG_CH0);
				command.AddParameterBasedOnDbColumn("@chargePk", chargePk, AccCommissionLineGroupSchema.CLG_AC);
				command.AddParameterBasedOnDbColumn("@transactionAmount", transactionAmount, AccCommissionLineGroupSchema.CLG_TransactionAmount);
				command.AddParameterBasedOnDbColumn("@transactionCurrency", transactionCurrency, AccCommissionLineGroupSchema.CLG_RX_NKTransactionCurrency);
				command.AddParameterBasedOnDbColumn("@totalCommissionableAmount", totalCommissionableAmount, AccCommissionLineGroupSchema.CLG_TotalCommissionableAmount);
				command.AddParameterBasedOnDbColumn("@commissionCurrency", commissionCurrency, AccCommissionLineGroupSchema.CLG_RX_NKCommissionCurrency);
				command.AddParameterBasedOnDbColumn("@commissionDate", commissionDate, AccCommissionLineGroupSchema.CLG_CommissionDate);
				command.AddParameterBasedOnDbColumn("@createTime", DateTime.UtcNow, AccCommissionLineGroupSchema.CLG_SystemCreateTimeUtc);
				command.AddParameterBasedOnDbColumn("@createUser", "AAA", AccCommissionLineGroupSchema.CLG_SystemCreateUser);
				command.AddParameterBasedOnDbColumn("@editTime", DateTime.UtcNow, AccCommissionLineGroupSchema.CLG_SystemLastEditTimeUtc);
				command.AddParameterBasedOnDbColumn("@editUser", "AAA", AccCommissionLineGroupSchema.CLG_SystemLastEditUser);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateAccGLHeader

		const string CreateAccGLHeaderSql = @"
INSERT INTO dbo.AccGLHeader(AG_PK, AG_AccountGroup, AG_AccountNum, AG_IsActive, AG_Description, AG_AccountType, AG_CashFlowType, AG_DebitCredit, AG_IsGlobal, AG_TotalLevel, AG_Column, AG_SystemCreateTimeUtc, AG_SystemCreateUser, AG_SystemLastEditTimeUtc, AG_SystemLastEditUser)
VALUES (@pk, @accountGroup, @accountNum, @isActive, @description, @accountType, @cashFlowType, @debitCredit, @isGlobal, @totalLevel, @column, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateAccGLHeader(string accountNum, string accountType, string debitCredit)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccGLHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@accountGroup", SqlDbType.VarChar, AccGLHeaderSchema.AG_AccountGroup.MaxLength, "");
				command.AddParameter("@accountNum", SqlDbType.VarChar, AccGLHeaderSchema.AG_AccountNum.MaxLength, accountNum);
				command.AddParameter("@isActive", SqlDbType.Bit, 1);
				command.AddParameter("@description", SqlDbType.VarChar, AccGLHeaderSchema.AG_Description.MaxLength, "TEST HEADER");
				command.AddParameter("@accountType", SqlDbType.Char, AccGLHeaderSchema.AG_AccountType.MaxLength, accountType);
				command.AddParameter("@cashFlowType", SqlDbType.VarChar, AccGLHeaderSchema.AG_CashFlowType.MaxLength, "XXX");
				command.AddParameter("@debitCredit", SqlDbType.Char, AccGLHeaderSchema.AG_DebitCredit.MaxLength, debitCredit);
				command.AddParameter("@isGlobal", SqlDbType.Bit, 1);
				command.AddParameter("@totalLevel", SqlDbType.Int, 0);
				command.AddParameter("@column", SqlDbType.Char, AccGLHeaderSchema.AG_Column.MaxLength, "AS");
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateAccPayableOrderHeader

		public Guid CreateAccPayableOrderHeader(Guid buyerPK, Guid companyPK, string orderNumber, string stage, string user, string aphType)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccPayableOrderHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@buyerPK", SqlDbType.UniqueIdentifier, buyerPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@systemCreateTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.AddParameter("@disposition", SqlDbType.VarChar, AccPayableOrderHeaderSchema.APH_Disposition.MaxLength, "TST");
				command.AddParameter("@goodsReceivedStatus", SqlDbType.VarChar, AccPayableOrderHeaderSchema.APH_GoodsReceivedStatus.MaxLength, "TST");
				command.AddParameter("@orderNumber", SqlDbType.VarChar, AccPayableOrderHeaderSchema.APH_OrderNumber.MaxLength, orderNumber);
				command.AddParameter("@stage", SqlDbType.VarChar, AccPayableOrderHeaderSchema.APH_Stage.MaxLength, stage);
				command.AddParameter("@user", SqlDbType.VarChar, AccPayableOrderHeaderSchema.APH_SystemCreateUser.MaxLength, user);
				command.AddParameter("@aphType", SqlDbType.VarChar, AccPayableOrderHeaderSchema.APH_Type.MaxLength, aphType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateAccPayableOrderHeaderSql = @"INSERT INTO dbo.AccPayableOrderHeader(APH_PK, APH_OA_Buyer, APH_GC, APH_Disposition, APH_GoodsReceivedStatus, APH_OrderNumber, APH_Stage, APH_SystemCreateUser, APH_Type, APH_SystemCreateTimeUtc)
			VALUES (@pk, @buyerPK, @companyPK, @disposition, @goodsReceivedStatus, @orderNumber, @stage, @user, @aphType, @systemCreateTimeUtc)";

		#endregion

		#region CreateAccTransactionHeader

		const string CreateAccTransactionHeaderSql = @"INSERT INTO dbo.AccTransactionHeader(AH_PK, AH_TransactionNum, AH_Ledger, AH_TransactionType, AH_InvoiceDate, AH_GB, AH_GC, AH_GE, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser) VALUES (@pk, @transNum, @ledger, @transactionType, GetUtcDate(), @gb, @gc, @ge, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateAccTransactionHeader(string ledger, string transactionType, Guid glbBranch, Guid glbCompany, Guid glbDepartment, string transactionNumber = null)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccTransactionHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@ledger", SqlDbType.Char, AccTransactionHeaderSchema.AH_Ledger.MaxLength, ledger);
				command.AddParameter("@transactionType", SqlDbType.Char, AccTransactionHeaderSchema.AH_TransactionType.MaxLength, transactionType);
				command.AddParameter("@gb", SqlDbType.UniqueIdentifier, glbBranch);
				command.AddParameter("@gc", SqlDbType.UniqueIdentifier, glbCompany);
				command.AddParameter("@ge", SqlDbType.UniqueIdentifier, glbDepartment);
				command.AddParameter("@transNum", SqlDbType.VarChar, AccTransactionHeaderSchema.AH_TransactionNum.MaxLength, transactionNumber ?? string.Empty);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateAccTransactionLines

		const string CreateAccTransactionLinesSql = @"INSERT INTO dbo.AccTransactionLines(AL_PK, AL_AH, AL_LineType, AL_LineAmount, AL_AG, AL_GC, AL_GB, AL_GE, AL_SystemCreateTimeUtc, AL_SystemCreateUser, AL_SystemLastEditTimeUtc, AL_SystemLastEditUser) VALUES (@pk, @headerPK, @lineType, @lineAmount, @glPK, @companyPK, @branchPK, @departmentPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateAccTransactionLines(Guid headerPK, string lineType, Guid glPK, Guid companyPK, Guid branchPK, Guid departmentPK, decimal lineAmount)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccTransactionLinesSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@headerPK", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@lineType", SqlDbType.Char, AccTransactionLinesSchema.AL_LineType.MaxLength, lineType);
				command.AddParameter("@lineAmount", SqlDbType.Decimal, lineAmount);
				command.AddParameter("@glPK", SqlDbType.UniqueIdentifier, glPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateAccTransactionMatchLink

		const string CreateAccTransactionMatchLinkSql = @"INSERT INTO dbo.AccTransactionMatchLink(AP_PK, AP_AH, AP_Amount, AP_MatchDate, AP_SystemCreateTimeUtc, AP_SystemCreateUser, AP_SystemLastEditTimeUtc, AP_SystemLastEditUser) VALUES (@pk, @headerPK, @amount, GetUtcDate(), GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateAccTransactionMatchLink(Guid headerPK, decimal amount)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccTransactionMatchLinkSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@headerPK", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@amount", SqlDbType.Decimal, amount);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateAccTransLinePay

		const string CreateAccTransLinePaySql = @"INSERT INTO dbo.AccTransLinePay(A7_PK, A7_AL, A7_AP, A7_Amount, A7_SystemCreateTimeUtc, A7_SystemCreateUser, A7_SystemLastEditTimeUtc, A7_SystemLastEditUser) VALUES (@pk, @linePK, @matchLinkPK, @amount, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateAccTransLinePay(Guid linePK, Guid matchLinkPK, decimal amount)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccTransLinePaySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@linePK", SqlDbType.UniqueIdentifier, linePK);
				command.AddParameter("@matchLinkPK", SqlDbType.UniqueIdentifier, matchLinkPK);
				command.AddParameter("@amount", SqlDbType.Decimal, amount);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region AccTransactionHeaderAuthorisationRecord

		const string AccTransactionHeaderAuthorisationRecordSql = @"
INSERT INTO dbo.AccTransactionHeaderAuthorisationRecord
(AHF_PK, AHF_RecordType, AHF_ParentId, AHF_ParentTableCode, AHF_Number, AHF_Counter, AHF_IDType, AHF_DateTime,        AHF_SystemCreateTimeUtc, AHF_SystemCreateUser, AHF_SystemLastEditTimeUtc, AHF_SystemLastEditUser)
VALUES
(@pk,    @recordType,    @parentId,    @parentTableCode,    '_',        '_',        '___',       SYSDATETIMEOFFSET(), GetUtcDate(),            '~BP',                GetUtcDate(),              '~BP')";

		public Guid CreateAccTransactionHeaderAuthorisationRecord(string recordType, Guid parentId, string parentTableCode)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(AccTransactionHeaderAuthorisationRecordSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@recordType", SqlDbType.Char, AccTransactionHeaderAuthorisationRecordSchema.AHF_RecordType.MaxLength, recordType);
				command.AddParameter("@parentId", SqlDbType.UniqueIdentifier, parentId);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, AccTransactionHeaderAuthorisationRecordSchema.AHF_ParentTableCode.MaxLength, parentTableCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateCarrierPreferredRoute

		public Guid CreateCarrierPreferredRoute(Guid fromAddress, Guid toAddress, bool isExclusive = true)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateCarrierPreferredRouteSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@fromAddress", SqlDbType.UniqueIdentifier, fromAddress);
				command.AddParameter("@toAddress", SqlDbType.UniqueIdentifier, toAddress);
				command.AddParameter("@isExclusive", SqlDbType.Bit, isExclusive);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateCarrierPreferredRouteSql =
			"INSERT INTO dbo.CarrierPreferredRoute (CPU_PK, CPU_IsExclusive,CPU_OA_FromAddress, CPU_OA_ToAddress, CPU_SystemCreateTimeUtc, CPU_SystemCreateUser, CPU_SystemLastEditTimeUtc, CPU_SystemLastEditUser) VALUES (@pk, @isExclusive, @fromAddress, @toAddress, GetUtcDate(), '---', GetUtcDate(), '---')";

		#endregion

		#region CreateCarrierPreferredRouteSegmentDivot

		public Guid CreateCarrierPreferredRouteSegmentDivot(Guid route, Guid routeSegment, int legNo)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateCarrierPreferredRouteSegmentDivotSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@route", SqlDbType.UniqueIdentifier, route);
				command.AddParameter("@routeSegment", SqlDbType.UniqueIdentifier, routeSegment);
				command.AddParameter("@legNo", SqlDbType.Int, legNo);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateCarrierPreferredRouteSegmentDivotSql =
			"INSERT INTO dbo.CarrierPreferredRouteSegmentDivot (CPG_PK, CPG_CPU_CarrierPreferredRoute, CPG_RSG_RouteSegment, CPG_LegNo, CPG_SystemCreateTimeUtc, CPG_SystemCreateUser, CPG_SystemLastEditTimeUtc, CPG_SystemLastEditUser) VALUES (@pK, @route, @routeSegment, @legNo, GetUtcDate(), '---', GetUtcDate(), '---')";

		#endregion

		#region CreateCarrierShipment

		public Guid CreateCarrierShipment(String carrierShipmentReference = "CA-TEST-000000000001")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateCarrierShipmentSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@reference", SqlDbType.VarChar, carrierShipmentReference);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateCarrierShipmentSql = "INSERT INTO dbo.CarrierShipmentHeader (CSH_PK, CSH_CarrierShipmentReference, CSH_SystemCreateTimeUtc, CSH_SystemCreateUser, CSH_SystemLastEditTimeUtc, CSH_SystemLastEditUser) VALUES (@pk, @reference, GetUtcDate(), '---', GetUtcDate(), '---')";

		#endregion

		#region CreateCarrierShipmentRouteLeg

		public Guid CreateCarrierShipmentRouteLeg(Guid carrierShipment, Guid fromAddress = new(), Guid toAddress = new(), string transportMode = "SEA", byte sequence = 1)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateCarrierShipmentRouteLegSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@carrierShipment", SqlDbType.UniqueIdentifier, carrierShipment);
				command.AddParameter("@sequence", SqlDbType.TinyInt, sequence);
				command.AddParameter("@distance", SqlDbType.Int, 2000);
				command.AddParameter("@distanceUnit", SqlDbType.VarChar, "NM");
				command.AddParameter("@fromAddress", SqlDbType.UniqueIdentifier, fromAddress);
				command.AddParameter("@toAddress", SqlDbType.UniqueIdentifier, toAddress);
				command.AddParameter("@transportMode", SqlDbType.Char, transportMode);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateCarrierShipmentRouteLegSql = "INSERT INTO dbo.CarrierShipmentRouteLeg (CRG_PK, CRG_CSH_CarrierShipment, CRG_Sequence, CRG_Distance, CRG_DistanceUnit, CRG_OA_FromAddress, CRG_OA_ToAddress, CRG_TransportMode, CRG_SystemCreateTimeUtc, CRG_SystemCreateUser, CRG_SystemLastEditTimeUtc, CRG_SystemLastEditUser) VALUES (@pk, @carrierShipment, @sequence, @distance, @distanceUnit, @fromAddress, @toAddress, @transportMode, GetUtcDate(), '---', GetUtcDate(), '---')";

		#endregion

		#region CreateRouteSegment

		public Guid CreateRouteSegment(Guid transportOperator, Guid fromAddress, Guid toAddress, string transportMode = "SEA", int distance = 5, string distanceUnit = "NM")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRouteSegmentSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@distance", SqlDbType.Int, distance);
				command.AddParameter("@distanceUnit", SqlDbType.VarChar, distanceUnit);
				command.AddParameter("@transportOperator", SqlDbType.UniqueIdentifier, transportOperator);
				command.AddParameter("@fromAddress", SqlDbType.UniqueIdentifier, fromAddress);
				command.AddParameter("@toAddress", SqlDbType.UniqueIdentifier, toAddress);
				command.AddParameter("@segmentID", SqlDbType.VarChar, Guid.NewGuid().ToString().Substring(0, 20));
				command.AddParameter("@transportMode", SqlDbType.VarChar, transportMode);
				command.AddParameter("@transitTime", SqlDbType.SmallDateTime, new DateTime(1901, 1, 2, 8, 30, 0));

				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateRouteSegmentSql =
			"INSERT INTO dbo.RouteSegment (RSG_PK, RSG_Distance, RSG_DistanceUnit, RSG_OH_TransportOperator, RSG_OA_FromAddress, RSG_OA_ToAddress, RSG_SegmentID, RSG_TransportMode, RSG_TransitTime, RSG_ValidFrom, RSG_SystemCreateTimeUtc, RSG_SystemCreateUser, RSG_SystemLastEditTimeUtc, RSG_SystemLastEditUser) VALUES (@pk, @distance, @distanceUnit, @transportOperator, @fromAddress, @toAddress, @segmentID, @transportMode, @transitTime, GetUtcDate(), GetUtcDate(), '---', GetUtcDate(), '---')";

		#endregion

		#region CreateContainerLoadListHeader

		public Guid CreateContainerLoadListHeader(string loadListId, Guid loadListParty, string loadMode = "CY")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateContainerLoadListHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@loadListId", SqlDbType.VarChar, ContainerLoadListHeaderSchema.CLH_LoadListId.MaxLength, loadListId);
				command.AddParameter("@loadMode", SqlDbType.VarChar, loadMode);
				command.AddParameter("@loadListParty", SqlDbType.UniqueIdentifier, loadListParty);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateContainerLoadListHeaderSql = "INSERT INTO dbo.ContainerLoadListHeader(CLH_PK, CLH_LoadListId, CLH_LoadMode, CLH_OH_LoadListParty, CLH_PlannedTransportMode, CLH_SystemCreateTimeUtc, CLH_SystemCreateUser, CLH_SystemLastEditTimeUtc, CLH_SystemLastEditUser) VALUES (@pk, @loadListId, @loadMode, @loadListParty, 'AIR', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateCYContainerLoadListLine

		public Guid CreateCYContainerLoadListLine(Guid loadListHeaderPK, Guid supplierBookingLinePK, Guid jobContainerPK, decimal packedQuantity)
		{
			var result = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateCYContainerLoadListLineSql))
			{
				command.AddParameter("@cllPK", SqlDbType.UniqueIdentifier, result);
				command.AddParameter("@loadListHeaderPK", SqlDbType.UniqueIdentifier, loadListHeaderPK);
				command.AddParameter("@supplierBookingLinePK", SqlDbType.UniqueIdentifier, supplierBookingLinePK);
				command.AddParameter("@jobContainerPK", SqlDbType.UniqueIdentifier, jobContainerPK);
				command.AddParameter("@loadMode", SqlDbType.VarChar, ContainerLoadListLineSchema.CLL_LoadMode.MaxLength, "CY");
				command.AddParameter("@packedQuantity", SqlDbType.Decimal, packedQuantity);
				command.ExecuteNonQuery();
			}

			return result;
		}

		const string CreateCYContainerLoadListLineSql = "INSERT INTO dbo.ContainerLoadListLine(CLL_PK, CLL_CLH_LoadListHeader, CLL_JSL_BookingLine, CLL_JC_Container, CLL_LoadMode, CLL_SystemCreateTimeUtc, CLL_SystemCreateUser, CLL_SystemLastEditTimeUtc, CLL_SystemLastEditUser, CLL_PackedQuantity) VALUES (@cllPK, @loadListHeaderPK, @supplierBookingLinePK, @jobContainerPK, @loadMode, GETDATE(), 'ZZ', GETDATE(), 'ZZ', @packedQuantity)";

		#endregion

		#region CreateRatingContract

		public Guid CreateRatingContract(string contractID, Guid serviceProviderPK, string contractType, string transportMode, string contractOwner = "~BP")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRatingContractSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@contractNumber", SqlDbType.VarChar, contractID);
				command.AddParameter("@serviceProvider", SqlDbType.UniqueIdentifier, serviceProviderPK);
				command.AddParameter("@contractType", SqlDbType.VarChar, contractType);
				command.AddParameter("@transportMode", SqlDbType.VarChar, transportMode);
				command.AddParameter("@contractOwner", SqlDbType.VarChar, contractOwner);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateRatingContractSql = @"INSERT INTO dbo.RatingContract(RCT_PK, RCT_ContractNumber, RCT_OH, RCT_StartDate, RCT_ContractType, RCT_TransportMode, RCT_SystemCreateTimeUtc, RCT_SystemCreateUser, RCT_SystemLastEditTimeUtc, RCT_SystemLastEditUser, RCT_GS_NKContractOwner)
VALUES (@pk, @contractNumber, @serviceProvider, GetUtcDate(), @contractType, @transportMode, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @contractOwner)";

		#endregion

		#region CreateRatingContractAllocationLine

		public Guid CreateRatingContractAllocationLine(string allocationID, Guid ratingContractPK, int allocatedQuantity, string loadLocation, string dischargeLocation)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRatingContractAllocationLineSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@allocationID", SqlDbType.VarChar, allocationID);
				command.AddParameter("@ratingContract", SqlDbType.UniqueIdentifier, ratingContractPK);
				command.AddParameter("@allocatedQuantity", SqlDbType.Int, allocatedQuantity);
				command.AddParameter("@loadLocation", SqlDbType.VarChar, loadLocation);
				command.AddParameter("@dischargeLocation", SqlDbType.VarChar, dischargeLocation);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateRatingContractAllocationLineSql = @"INSERT INTO dbo.RatingContractAllocationLine(RCA_PK, RCA_RCT_RatingContract, RCA_StartDate, RCA_ExpiryDate, RCA_LoadLocation, RCA_DischargeLocation, RCA_VoyageNumber, RCA_RV_NKVessel, RCA_AllocatedQuantity, RCA_AllocatedUQ, RCA_AllocationLineID, RCA_SystemCreateTimeUtc, RCA_SystemCreateUser, RCA_SystemLastEditTimeUtc, RCA_SystemLastEditUser)
VALUES(@pk, @ratingContract, GetUtcDate(), GetUtcDate(), @loadLocation, @dischargeLocation, '', '', @allocatedQuantity, 'TU', @allocationID, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateAllocationRouteWizardTemplateHeader

		public Guid CreateAllocationRouteWizardTemplateHeader(string name)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateAllocationRouteWizardTemplateHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@name", SqlDbType.VarChar, name);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateAllocationRouteWizardTemplateHeaderSql = @"INSERT INTO dbo.AllocationRouteWizardTemplateHeader(ARH_PK, ARH_Name, ARH_SystemCreateTimeUtc, ARH_SystemCreateUser, ARH_SystemLastEditTimeUtc, ARH_SystemLastEditUser)
VALUES(@pk, @name, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateAllocationRouteWizardTemplateRow

		public Guid CreateAllocationRouteWizardTemplateRow(Guid allocationRouteWizardTemplateHeader, int sequence, bool hasLinkedSchedule = false)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateAllocationRouteWizardTemplateRowSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@header", SqlDbType.UniqueIdentifier, allocationRouteWizardTemplateHeader);
				command.AddParameter("@sequence", SqlDbType.SmallInt, sequence);
				command.AddParameter("@hasLinkedSchedule", SqlDbType.Bit, hasLinkedSchedule);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateAllocationRouteWizardTemplateRowSql = @"INSERT INTO dbo.AllocationRouteWizardTemplateRow(ARR_PK, ARR_ARH_AllocationRouteWizardTemplateHeader, ARR_Sequence, ARR_HasLinkedSchedule, ARR_SystemCreateTimeUtc, ARR_SystemCreateUser, ARR_SystemLastEditTimeUtc, ARR_SystemLastEditUser)
VALUES(@pk, @header, @sequence, @hasLinkedSchedule, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateOrgHeader

		public Guid CreateOrgHeader(string code, string fullName, bool isConsignor = false, bool isForwarder = false)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateOrgSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, OrgHeaderSchema.OH_Code.MaxLength, code);
				command.AddParameter("@fullName", SqlDbType.VarChar, OrgHeaderSchema.OH_FullName.MaxLength, fullName);
				command.AddParameter("@isConsignor", SqlDbType.Bit, isConsignor);
				command.AddParameter("@isForwarder", SqlDbType.Bit, isForwarder);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgSecurity

		public Guid CreateOrgSecurity(string securityItemName, Guid orgPK, bool isGranted = false, Guid? stmMenuItemPK = null)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateOrgSecuritySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@itemName", SqlDbType.VarChar, OrgSecuritySchema.OX_SecurityItemName.MaxLength, securityItemName);
				command.AddParameter("@orgPK", SqlDbType.UniqueIdentifier, OrgSecuritySchema.OX_OH.MaxLength, orgPK);
				command.AddParameter("@isGranted", SqlDbType.Bit, OrgSecuritySchema.OX_Granted.MaxLength, isGranted);
				command.AddParameter("@stmMenuItemPK", SqlDbType.UniqueIdentifier, OrgSecuritySchema.OX_SU.MaxLength, (object)stmMenuItemPK ?? DBNull.Value);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgSecurityContact

		public Guid CreateOrgSecurityContact(Guid orgSecurityPK, Guid contactPK, bool isGranted = false)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateOrgSecurityContactsSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@securityPK", SqlDbType.UniqueIdentifier, OrgSecurityContactsSchema.OZ_OX.MaxLength, orgSecurityPK);
				command.AddParameter("@contactPK", SqlDbType.UniqueIdentifier, OrgSecurityContactsSchema.OZ_OC.MaxLength, contactPK);
				command.AddParameter("@isGranted", SqlDbType.Bit, OrgSecurityContactsSchema.OZ_Granted.MaxLength, isGranted);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateAccPayableOrderLine

		public Guid CreateAccPayableOrderLine(Guid headerPK, Guid branchPK, Guid companyPK, Guid departmentPK, string packTypePK)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateAccPayableOrderLineSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@headerPK", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				command.AddParameter("@packType", SqlDbType.VarChar, AccPayableOrderLineSchema.APL_F3_NKPackType.MaxLength, packTypePK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateAccPayableOrderLineSql = @"
INSERT INTO dbo.AccPayableOrderLine(APL_PK, APL_APH, APL_GB, APL_GC, APL_GE, APL_F3_NKPackType, APL_SystemCreateTimeUtc, APL_SystemCreateUser, APL_SystemLastEditTimeUtc, APL_SystemLastEditUser)
VALUES (@pk, @headerPK, @branchPK, @companyPK, @departmentPK, @packType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateBPMConfigurationTmpl

		public Guid CreateBPMConfigurationTmpl(string parentTableCode, Guid parentID, Guid departmentPK, Guid branchPK, Guid companyPK, Guid clientPK)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateBPMConfigurationTmplSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, BPMConfigurationTmplSchema.VCT_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, departmentPK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@clientPK", SqlDbType.UniqueIdentifier, clientPK);
				command.AddParameter("@createTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.AddParameter("@editTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateBPMConfigurationTmplSql = @"INSERT INTO dbo.BPMConfigurationTmpl(VCT_PK, VCT_ParentTableCode, VCT_ParentID, VCT_GE_Department, VCT_GB_Branch, VCT_GC_Company, VCT_OH_Client, VCT_SystemCreateTimeUtc, VCT_SystemLastEditTimeUtc)
			VALUES (@pk, @parentTableCode, @parentID, @departmentPK, @branchPK, @companyPK, @clientPK, @createTimeUtc, @editTimeUtc)";

		#endregion

		#region CreateUNDGDataItem

		public Guid CreateUNDGDataItem(string parentTableCode, Guid parentID, string packType = "UNT")
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateUNDGDataItemSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, UNDGDataItemSchema.DI_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@packType", SqlDbType.VarChar, UNDGDataItemSchema.DI_F3_NKPackType.MaxLength, packType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateUNDGDataItemSql = @"
INSERT INTO dbo.UNDGDataItem(DI_PK, DI_ParentTableCode, DI_ParentID, DI_F3_NKPackType, DI_SystemCreateTimeUtc, DI_SystemCreateUser, DI_SystemLastEditTimeUtc, DI_SystemLastEditUser)
VALUES (@pk, @parentTableCode, @parentID, @packType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateSupplierBookingLineWithPackType

		public Guid CreateSupplierBookingLineWithPackType(Guid headerPK, string packType)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateSupplierBookingLineWithPackTypeSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@headerPK", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@packType", SqlDbType.VarChar, SupplierBookingLineSchema.DL_F3_NKPackType.MaxLength, packType);
				command.AddParameter("@DL_SystemCreateTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.AddParameter("@DL_SystemLastEditTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateSupplierBookingLineWithPackTypeSql = @"INSERT INTO dbo.SupplierBookingLine(DL_PK, DL_DH_BookingHeader, DL_F3_NKPackType, DL_SystemCreateTimeUtc, DL_SystemLastEditTimeUtc) VALUES (@pk, @headerPK, @packType, @DL_SystemCreateTimeUtc, @DL_SystemLastEditTimeUtc)";

		#endregion

		#region CreateJobShipmentPreplanning

		public Guid CreateJobShipmentPreplanning(Guid buyerAddressPK, string preshipID, string packType)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateJobShipmentPreplanningSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@buyerAddressPK", SqlDbType.UniqueIdentifier, buyerAddressPK);
				command.AddParameter("@preshipID", SqlDbType.VarChar, JobShipmentPreplanningSchema.EF_PreshipID.MaxLength, preshipID);
				command.AddParameter("@packType", SqlDbType.VarChar, JobShipmentPreplanningSchema.EF_F3_NKPackType.MaxLength, packType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateJobShipmentPreplanningSql = @"INSERT INTO dbo.JobShipmentPreplanning(EF_PK, EF_OA_BuyerAddress, EF_PreshipID, EF_F3_NKPackType, EF_SystemCreateTimeUtc, EF_SystemCreateUser, EF_SystemLastEditTimeUtc, EF_SystemLastEditUser) VALUES (@pk, @buyerAddressPK, @preshipID, @packType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region Create HVLV/ETails/ECommerce Tables

		public Guid CreateHVLVBookingHeader(Guid billToPartyAddress, string reference, string user, int clusterKey)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateHVLVBookingHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@billToPartyAddress", SqlDbType.UniqueIdentifier, billToPartyAddress);
				command.AddParameter("@reference", SqlDbType.VarChar, HVLVBookingHeaderSchema.HVH_BookingReference.MaxLength, reference);
				command.AddParameter("@user", SqlDbType.VarChar, HVLVBookingHeaderSchema.HVH_SystemCreateUser.MaxLength, user);
				command.AddParameter("@cluster_Key", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateHVLVBookingHeaderSql = @"INSERT INTO dbo.HVLVBookingHeader(HVH_PK, HVH_ClusterKey, HVH_OA_BillToParty, HVH_BookingReference, HVH_SystemCreateUser, HVH_SystemLastEditUser, HVH_SystemCreateTimeUtc, HVH_SystemLastEditTimeUtc) VALUES (@pk, @cluster_Key, @billToPartyAddress, @reference, @user, @user, GETUTCDATE(), GETUTCDATE())";

		public Guid CreateHVLVOriginLoadList(string reference, string user = "~BP", Guid originDepotAddressPK = new Guid(), Guid ownerPK = new Guid())
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateHVLVOriginLoadListSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@uniqueReference", SqlDbType.VarChar, HVLVOriginLoadListSchema.HVL_UniqueReference.MaxLength, reference);
				command.AddParameter("@user", SqlDbType.VarChar, HVLVBookingHeaderSchema.HVH_SystemCreateUser.MaxLength, user);
				command.AddParameter("@originDepotAddressPK", SqlDbType.UniqueIdentifier, originDepotAddressPK == Guid.Empty ? DBNull.Value : originDepotAddressPK);
				command.AddParameter("@ownerPK", SqlDbType.UniqueIdentifier, ownerPK == Guid.Empty ? DBNull.Value : ownerPK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateHVLVOriginLoadListSql = @"INSERT INTO dbo.HVLVOriginLoadList (HVL_PK, HVL_OA_OriginDepot, HVL_OH_Owner, HVL_UniqueReference, HVL_SystemCreateTimeUtc, HVL_SystemCreateUser, HVL_SystemLastEditTimeUtc, HVL_SystemLastEditUser) VALUES (@pk, @originDepotAddressPK, @ownerPK, @uniqueReference, GETUTCDATE(), @user, GETUTCDATE(), @user)";

		public Guid CreateHVLVConsignment(Guid bookingPK, string reference, string consignmentId, string user, int itemCount, int clusterKey, string importCustomsClearanceStatus = "", DateTime createTime = default, Guid? consignmentHeaderPK = null, string waybillNumber = "", bool isActive = true, string status = "BKD", string consigneePostcode = "", string shipperPostcode = "", string returnPostcode = "")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateHVLVConsignmentSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@bookingPK", SqlDbType.UniqueIdentifier, bookingPK);
				command.AddParameter("@consignmentHeaderPK", SqlDbType.UniqueIdentifier, consignmentHeaderPK == null ? DBNull.Value : consignmentHeaderPK);
				command.AddParameter("@reference", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_ShipperReference.MaxLength, reference);
				command.AddParameter("@consignmentId", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_ConsignmentId.MaxLength, consignmentId);
				command.AddParameter("@user", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_SystemCreateUser.MaxLength, user);
				command.AddParameter("@createTimeUtc", SqlDbType.DateTime, createTime == default ? DateTime.Now : createTime);
				command.AddParameter("@editTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.AddParameter("@itemCount", SqlDbType.Int, itemCount);
				command.AddParameter("@cluster_Key", SqlDbType.Int, clusterKey);
				command.AddParameter("@importCustomsClearanceStatus", SqlDbType.VarChar, importCustomsClearanceStatus);
				command.AddParameter("@waybillNumber", SqlDbType.VarChar, HVLVConsignmentSchema.HVC_WaybillNumber.MaxLength, waybillNumber);
				command.AddParameter("@isActive", SqlDbType.Bit, isActive);
				command.AddParameter("@status", SqlDbType.VarChar, status);
				command.AddParameter("@consigneePostcode", SqlDbType.NVarChar, consigneePostcode);
				command.AddParameter("@shipperPostcode", SqlDbType.VarChar, shipperPostcode);
				command.AddParameter("@returnPostcode", SqlDbType.NVarChar, returnPostcode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateHVLVConsignmentSql = @"INSERT INTO dbo.HVLVConsignment(HVC_PK, HVC_ClusterKey, HVC_HVH_BookingHeader, HVC_HCH_Header, HVC_ShipperReference, HVC_ConsignmentId, HVC_WaybillNumber, HVC_SystemCreateUser, HVC_SystemLastEditUser, HVC_SystemCreateTimeUtc, HVC_SystemLastEditTimeUtc, HVC_GoodsValue, HVC_Status, HVC_ItemCount, HVC_ImportCustomsClearanceStatus, HVC_IsActive, HVC_ConsigneePostcode, HVC_ShipperPostcode, HVC_ReturnPostcode) VALUES (@pk, @cluster_Key, @bookingPK, @consignmentHeaderPK, @reference, @consignmentId, @waybillNumber, @user, @user, @createTimeUtc, @editTimeUtc, 0, @status, @itemCount, @importCustomsClearanceStatus, @isActive, @consigneePostcode, @shipperPostcode, @returnPostcode)";

		public Guid CreateHVLVConsignmentHeader(int clusterKey, string jobNumber, Guid shipmentPK, DateTime createTime = default)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateHVLVConsignmentHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@jobNumber", SqlDbType.VarChar, jobNumber);
				command.AddParameter("@shipmentPk", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@createTimeUtc", SqlDbType.DateTime, createTime == default ? DateTime.Now : createTime);
				command.AddParameter("@createUser", SqlDbType.VarChar, "AAA");
				command.AddParameter("@lastEditTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.AddParameter("@lastEditUser", SqlDbType.VarChar, "AAA");
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateHVLVConsignmentHeaderSql = @"INSERT INTO dbo.HVLVConsignmentHeader(HCH_PK, HCH_ClusterKey, HCH_JobNumber, HCH_JS_Shipment, HCH_SystemCreateTimeUtc, HCH_SystemCreateUser, HCH_SystemLastEditTimeUtc, HCH_SystemLastEditUser) VALUES (@pk, @clusterKey, @jobNumber, @shipmentPk, @createTimeUtc, @createUser, @lastEditTimeUtc, @lastEditUser)";

		public Guid CreateHVLVHVLVDeliveryByArea(string consigneePostcode = "", DateTime systemCreateTimeUtc = default, DateTime systemLastEditTimeUtc = default, int consignmentCount = 1)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateHVLVDeliveryByAreaSql))
			{
				command.AddParameter("@pK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@consigneePostcode", SqlDbType.NVarChar, consigneePostcode);
				command.AddParameter("@systemCreateTimeUtc", SqlDbType.SmallDateTime, systemCreateTimeUtc == default ? DateTime.Now : systemCreateTimeUtc);
				command.AddParameter("@systemLastEditTimeUtc", SqlDbType.SmallDateTime, systemLastEditTimeUtc == default ? DateTime.Now : systemLastEditTimeUtc);
				command.AddParameter("@systemCreateUser", SqlDbType.VarChar, "~BP");
				command.AddParameter("@systemLastEditUser", SqlDbType.VarChar, "~BP");
				command.AddParameter("@createdMonth", SqlDbType.TinyInt, 1);
				command.AddParameter("@createdYear", SqlDbType.SmallInt, 2021);
				command.AddParameter("@consignmentCount", SqlDbType.Int, consignmentCount);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateHVLVDeliveryByAreaSql = @"INSERT INTO dbo.HVLVDeliveryByArea (HDB_PK, HDB_ConsigneePostcode, HDB_SystemCreateTimeUtc, HDB_SystemLastEditTimeUtc, HDB_SystemCreateUser, HDB_SystemLastEditUser, HDB_CreatedMonth, HDB_CreatedYear, HDB_ConsignmentCount) VALUES (@pK, @consigneePostcode, @systemCreateTimeUtc, @systemLastEditTimeUtc, @systemCreateUser, @systemLastEditUser, @createdMonth, @createdYear, @consignmentCount)";

		public Guid CreateHVLVItem(Guid consignmentPK, string packType, string itemID, int clusterKey, string usageType = "S", DateTime shipperFirstUsageTime = default, DateTime originFirstUsageTime = default, DateTime destinationFirstUsageTime = default, Guid loadedOnShipmentPK = default, Guid outerPackagePK = default)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateHVLVItemSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@consignmentPK", SqlDbType.UniqueIdentifier, consignmentPK);
				command.AddParameter("@packType", SqlDbType.VarChar, HVLVItemSchema.HVI_F3_NKPackType.MaxLength, packType);
				command.AddParameter("@itemID", SqlDbType.VarChar, HVLVItemSchema.HVI_ItemId.MaxLength, itemID);
				command.AddParameter("@cluster_Key", SqlDbType.Int, clusterKey);
				command.AddParameter("@usageType", SqlDbType.VarChar, HVLVItemSchema.HVI_UsageType.MaxLength, usageType);
				command.AddParameter("@shipperFirstUsageTime", SqlDbType.DateTime, shipperFirstUsageTime == default ? DBNull.Value : shipperFirstUsageTime);
				command.AddParameter("@originFirstUsageTime", SqlDbType.DateTime, originFirstUsageTime == default ? DBNull.Value : originFirstUsageTime);
				command.AddParameter("@destinationFirstUsageTime", SqlDbType.DateTime, destinationFirstUsageTime == default ? DBNull.Value : destinationFirstUsageTime);
				command.AddParameter("@loadedOnShipmentPK", SqlDbType.UniqueIdentifier, loadedOnShipmentPK == default ? DBNull.Value : loadedOnShipmentPK);
				command.AddParameter("@outerPackagePK", SqlDbType.UniqueIdentifier, outerPackagePK == default ? DBNull.Value : outerPackagePK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateHVLVItemSql = @"INSERT INTO dbo.HVLVItem(HVI_PK, HVI_ClusterKey, HVI_HVC_Consignment, HVI_F3_NKPackType, HVI_ItemId, HVI_UsageType, HVI_ShipperFirstUsageTimeUtc, HVI_OriginFirstUsageTimeUtc, HVI_DestinationFirstUsageTimeUtc, HVI_JS_LoadedOnShipment, HVI_HVO_OuterPackage, HVI_SystemCreateTimeUtc, HVI_SystemCreateUser, HVI_SystemLastEditTimeUtc, HVI_SystemLastEditUser) VALUES (@pk, @cluster_Key, @consignmentPK, @packType, @itemID, @usageType, @shipperFirstUsageTime, @originFirstUsageTime, @destinationFirstUsageTime, @loadedOnShipmentPK, @outerPackagePK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateHVLVItemLine(Guid itemPK, int quantity, int clusterKey, string destinationTariff)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateHVLVItemLineSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@itemPK", SqlDbType.UniqueIdentifier, itemPK);
				command.AddParameter("@quantity", SqlDbType.Int, quantity);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@destinationTariff", SqlDbType.VarChar, HVLVItemLineSchema.HVS_DestinationTariff.MaxLength, destinationTariff);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateHVLVItemLineSql = @"INSERT INTO dbo.HVLVItemLine(HVS_PK, HVS_HVI_HVLVItem, HVS_Quantity, HVS_ClusterKey, HVS_DestinationTariff, HVS_SystemCreateTimeUtc, HVS_SystemCreateUser, HVS_SystemLastEditTimeUtc, HVS_SystemLastEditUser) VALUES (@pk, @itemPK, @quantity, @clusterKey, @destinationTariff, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateHVLVOuterPackage(Guid loadListPK, Guid ownerOrgPK, string barcode = "")
		{
			var outerPackagePK = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateHVLVOuterPackageSql))
			{
				command.AddParameter("@outerPackagePK", SqlDbType.UniqueIdentifier, outerPackagePK);
				command.AddParameter("@loadListPK", SqlDbType.UniqueIdentifier, loadListPK == Guid.Empty ? DBNull.Value : loadListPK);
				command.AddParameter("@ownerOrgPK", SqlDbType.UniqueIdentifier, ownerOrgPK == Guid.Empty ? DBNull.Value : ownerOrgPK);
				command.AddParameter("@barcode", SqlDbType.VarChar, HVLVOuterPackageSchema.HVO_PackageBarcode.MaxLength, barcode);
				command.ExecuteNonQuery();
			}

			return outerPackagePK;
		}

		const string CreateHVLVOuterPackageSql = @"INSERT INTO dbo.HVLVOuterPackage([HVO_PK], [HVO_HVL_LoadList], [HVO_OH_Owner], [HVO_PackageBarcode], [HVO_SystemCreateTimeUtc], [HVO_SystemCreateUser], [HVO_SystemLastEditTimeUtc], [HVO_SystemLastEditUser]) VALUES (@outerPackagePK, @loadListPK, @ownerOrgPK, @barcode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateHVLVUsage(Guid parentItem, string category, string code)
		{
			var usagePK = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateHVLVUsageSql))
			{
				command.AddParameter("@usagePK", SqlDbType.UniqueIdentifier, usagePK);
				command.AddParameter("@parentItem", SqlDbType.UniqueIdentifier, parentItem == Guid.Empty ? DBNull.Value : parentItem);
				command.AddParameter("@category", SqlDbType.VarChar, category);
				command.AddParameter("@code", SqlDbType.VarChar, code);
				command.ExecuteNonQuery();
			}

			return usagePK;
		}

		const string CreateHVLVUsageSql = @"INSERT INTO dbo.HVLVUsage([HXU_PK], [HXU_HVI_ParentItem], [HXU_Category], [HXU_GS_NKUser], [HXU_Code], [HXU_GC_NKCompany], [HXU_BranchCode], [HXU_UsageTimeUtc]) VALUES (@usagePK, @parentItem, @category, '~BP', @code, '~BP', '~BP', GetUtcDate())";

		public Guid CreateHVLVUsageHistory(Guid shipmentHistoryPK, string category, int count)
		{
			var usageHistoryPK = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateHVLVUsageHistorySql))
			{
				command.AddParameter("@usageHistoryPK", SqlDbType.UniqueIdentifier, usageHistoryPK);
				command.AddParameter("@shipmentHistoryPK", SqlDbType.UniqueIdentifier, shipmentHistoryPK == Guid.Empty ? DBNull.Value : shipmentHistoryPK);
				command.AddParameter("@category", SqlDbType.VarChar, category);
				command.AddParameter("@count", SqlDbType.Int, count);
				command.ExecuteNonQuery();
			}

			return usageHistoryPK;
		}

		const string CreateHVLVUsageHistorySql = @"INSERT INTO dbo.HVLVUsageHistory([HUS_PK], [HUS_HSH_ShipmentHistory], [HUS_Category], [HUS_Count]) VALUES (@usageHistoryPK, @shipmentHistoryPK, @category, @count)";

		#endregion Create HVLV/ETails/ECommerce Tables

		#region CreateJobOrderLineDelivery

		public Guid CreateJobOrderLineDelivery(Guid deliveryAddr)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateJobOrderLineDeliverySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@deliveryAddr", SqlDbType.UniqueIdentifier, deliveryAddr);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateJobOrderLineDeliverySql = @"INSERT INTO dbo.JobOrderLineDelivery(J4_PK, J4_OA_DeliveryAddr, J4_SystemCreateTimeUtc, J4_SystemCreateUser, J4_SystemLastEditTimeUtc, J4_SystemLastEditUser) VALUES (@pk, @deliveryAddr, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateJobOrderLineDeliverContainer

		public Guid CreateJobOrderLineDeliverContainer(Guid j4PK, string packType)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateJobOrderLineDeliverContainerSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@j4PK", SqlDbType.UniqueIdentifier, j4PK);
				command.AddParameter("@packType", SqlDbType.VarChar, JobOrderLineDeliverContainerSchema.J5_F3_NKPackType.MaxLength, packType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateJobOrderLineDeliverContainerSql = @"
INSERT INTO dbo.JobOrderLineDeliverContainer(J5_PK, J5_J4, J5_F3_NKPackType, J5_SystemCreateTimeUtc, J5_SystemCreateUser, J5_SystemLastEditTimeUtc, J5_SystemLastEditUser)
VALUES (@pk, @j4PK, @packType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateJobContainer

		public Guid CreateJobContainer(string containerMode, string packType)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateJobContainerSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@containerMode", SqlDbType.VarChar, JobContainerSchema.JC_ContainerMode.MaxLength, containerMode);
				command.AddParameter("@packType", SqlDbType.VarChar, JobContainerSchema.JC_F3_NKPackType.MaxLength, packType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateJobContainerSql = @"INSERT INTO dbo.JobContainer(JC_PK, JC_ContainerMode, JC_F3_NKPackType, JC_SystemCreateTimeUtc, JC_SystemCreateUser, JC_SystemLastEditTimeUtc, JC_SystemLastEditUser) VALUES (@pk, @containerMode, @packType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateJobOrderHeader

		public Guid CreateJobOrderHeader(Guid buyerAddressPK, string packType = "PLT", string orderNumber = "ORD001", string status = "INC")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobOrderHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@buyerAddressPK", SqlDbType.UniqueIdentifier, buyerAddressPK);
				command.AddParameter("@packType", SqlDbType.VarChar, JobOrderHeaderSchema.JD_F3_NKPackType.MaxLength, packType);
				command.AddParameter("@orderNumber", SqlDbType.VarChar, orderNumber);
				command.AddParameter("@status", SqlDbType.VarChar, JobOrderHeaderSchema.JD_OrderStatus.MaxLength, status);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateJobOrderHeaderSql = @"INSERT INTO dbo.JobOrderHeader(JD_PK, JD_OA_BuyerAddress, JD_F3_NKPackType, JD_OrderNumber, JD_OrderStatus, JD_SystemCreateTimeUtc, JD_SystemCreateUser, JD_SystemLastEditTimeUtc, JD_SystemLastEditUser) VALUES (@pk, @buyerAddressPK, @packType, @orderNumber, @status, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateJobPackLines

		public Guid CreateJobPackLines(Guid shipmentPK, Guid pk, string packType, string refNumber = "", string packLineId = "", bool isHighRisk = false)
		{
			using (DbCommand command = Db.Connection.Command(CreateJobPackLinesSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@shipmentPK", SqlDbType.UniqueIdentifier, shipmentPK);
				command.AddParameter("@packType", SqlDbType.VarChar, JobPackLinesSchema.JL_F3_NKPackType.MaxLength, packType);
				command.AddParameter("@refNumber", SqlDbType.VarChar, JobPackLinesSchema.JL_RefNumber.MaxLength, refNumber);
				command.AddParameter("@packLineId", SqlDbType.VarChar, JobPackLinesSchema.JL_PackLineId.MaxLength, packLineId);
				command.AddParameter("@isHighRisk", SqlDbType.Bit, isHighRisk);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateJobPackLinesSql = @"
INSERT INTO dbo.JobPackLines(JL_PK, JL_JS, JL_F3_NKPackType, JL_RefNumber, JL_PackLineId, JL_IsHighRisk, JL_SystemCreateTimeUtc, JL_SystemCreateUser, JL_SystemLastEditTimeUtc, JL_SystemLastEditUser)
VALUES (@pk, @shipmentPK, @packType, @refNumber, @packLineId, @isHighRisk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateJobOrderLine

		public Guid CreateJobOrderLine(Guid jdPK, string packType, int subLineNo = 0)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateJobOrderLineSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jdPK", SqlDbType.UniqueIdentifier, jdPK);
				command.AddParameter("@packType", SqlDbType.VarChar, JobOrderLineSchema.JO_F3_NKPackType.MaxLength, packType);
				command.AddParameter("@subLineNo", SqlDbType.Int, subLineNo);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateJobOrderLineSql = @"INSERT INTO dbo.JobOrderLine(JO_PK, JO_JD, JO_F3_NKPackType, JO_SubLineNo, JO_SystemCreateTimeUtc, JO_SystemCreateUser, JO_SystemLastEditTimeUtc, JO_SystemLastEditUser) VALUES (@pk, @jdPK, @packType, @subLineNo, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateJobShipment

		public Guid CreateJobShipment(string uniqueRef, string packType, string totalCountPackType, string shipmentType = "", Guid masterShipmentPK = default)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateJobShipmentSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@uniqueRef", SqlDbType.VarChar, JobShipmentSchema.JS_UniqueConsignRef.MaxLength, uniqueRef);
				command.AddParameter("@packType", SqlDbType.VarChar, JobShipmentSchema.JS_F3_NKPackType.MaxLength, packType);
				command.AddParameter("@totalCountPackType", SqlDbType.VarChar, JobShipmentSchema.JS_F3_NKTotalCountPackType.MaxLength, totalCountPackType);
				command.AddParameter("@shipmentType", SqlDbType.VarChar, JobShipmentSchema.JS_ShipmentType.MaxLength, shipmentType);
				command.AddParameter("@masterShipmentPK", SqlDbType.UniqueIdentifier, masterShipmentPK == default ? DBNull.Value : masterShipmentPK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateJobShipmentSql = @"INSERT INTO dbo.JobShipment(JS_PK, JS_UniqueConsignRef, JS_F3_NKPackType, JS_F3_NKTotalCountPackType, JS_ShipmentType, JS_JS_ColoadMasterShipment, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser) VALUES (@pk, @uniqueRef, @packType, @totalCountPackType, @shipmentType, @masterShipmentPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateBookingShipment

		public Guid CreateBookingShipment(string uniqueRef)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateBookingShipmentSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@uniqueRef", SqlDbType.VarChar, JobShipmentSchema.JS_UniqueConsignRef.MaxLength, uniqueRef);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateBookingShipmentSql = @"
INSERT INTO dbo.JobShipment(JS_PK, JS_UniqueConsignRef, JS_IsBooking, JS_IsForwardRegistered, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser)
VALUES (@pk, @uniqueRef, 1, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreatePackageJob

		public Guid CreatePackageJob(string jobID, Guid parentID, string parentTableCode)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreatePackageJobSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobID", SqlDbType.VarChar, PkgPackageJobSchema.KJ_JobID.MaxLength, jobID);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, PkgPackageJobSchema.KJ_ParentTableCode.MaxLength, parentTableCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreatePackageJobSql = @"INSERT INTO dbo.PkgPackageJob(KJ_PK, KJ_JobID, KJ_ParentID, KJ_ParentTableCode, KJ_SystemCreateTimeUtc, KJ_SystemCreateUser, KJ_SystemLastEditTimeUtc, KJ_SystemLastEditUser) VALUES (@pk, @jobID, @parentID, @parentTableCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreatePkgPackageContainer

		public Guid CreatePkgPackageContainer(Guid packagePK, Guid containerTypePK, decimal tareWeight, decimal dunnageWeight)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreatePkgPackageContainerSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@packagePK", SqlDbType.UniqueIdentifier, packagePK);
				command.AddParameter("@containerTypePK", SqlDbType.UniqueIdentifier, containerTypePK);
				command.AddParameter("@tareWeight", SqlDbType.Decimal, tareWeight);
				command.AddParameter("@dunnageWeight", SqlDbType.Decimal, dunnageWeight);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreatePkgPackageContainerSql = @"INSERT INTO dbo.PkgPackageContainer(K0_PK, K0_KP_Package, K0_RC_ContainerType, K0_TareWeight, K0_DunnageWeight, K0_SystemCreateTimeUtc, K0_SystemCreateUser, K0_SystemLastEditTimeUtc, K0_SystemLastEditUser) VALUES (@pk, @packagePK, @containerTypePK, @tareWeight, @dunnageWeight, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')";

		#endregion

		#region CreatePkgPackage

		public Guid CreatePkgPackage(Guid packageJob, string packType, Guid parentPackagePK)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreatePkgPackageWithParentPackageSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@packageJob", SqlDbType.UniqueIdentifier, packageJob);
				command.AddParameter("@sequence", SqlDbType.Int, 0);
				command.AddParameter("@packType", SqlDbType.VarChar, packType);
				command.AddParameter("@parentPackage", SqlDbType.UniqueIdentifier, parentPackagePK);
				command.ExecuteNonQuery();
			}

			return pk;
		}
		const string CreatePkgPackageWithParentPackageSql = @"
INSERT INTO dbo.PkgPackage(KP_PK, KP_KJ_ParentPackageJob, KP_Sequence, KP_F3_NKPackType, KP_KP_ParentPackage, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser)
VALUES (@pk, @packageJob, @sequence, @packType, @parentPackage, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreatePkgPackage(Guid packageJob, string packType, int sequence)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreatePkgPackageSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@packageJob", SqlDbType.UniqueIdentifier, packageJob);
				command.AddParameter("@sequence", SqlDbType.Int, sequence);
				command.AddParameter("@packType", SqlDbType.VarChar, PkgPackageSchema.KP_F3_NKPackType.MaxLength, packType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreatePkgPackageSql = @"INSERT INTO dbo.PkgPackage(KP_PK, KP_KJ_ParentPackageJob, KP_Sequence, KP_F3_NKPackType, KP_SystemCreateTimeUtc, KP_SystemCreateUser, KP_SystemLastEditTimeUtc, KP_SystemLastEditUser) VALUES (@pk, @packageJob, @sequence, @packType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateOrgProductType

		public Guid CreateOrgProductType(Guid owner, string code, string packType)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateOrgProductTypeSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@owner", SqlDbType.UniqueIdentifier, owner);
				command.AddParameter("@code", SqlDbType.VarChar, OrgProductTypeSchema.OPT_Code.MaxLength, code);
				command.AddParameter("@packType", SqlDbType.VarChar, OrgProductTypeSchema.OPT_F3_NKPackType.MaxLength, packType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateOrgProductTypeSql = @"
INSERT INTO dbo.OrgProductType(OPT_PK, OPT_OH_Owner, OPT_Code, OPT_F3_NKPackType, OPT_SystemCreateTimeUtc, OPT_SystemCreateUser, OPT_SystemLastEditTimeUtc, OPT_SystemLastEditUser)
VALUES (@pk, @owner, @code, @packType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateOrgSales

		public Guid CreateOrgSales()
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateOrgSalesSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateOrgSalesSql = @"INSERT INTO dbo.OrgSales(OW_PK, OW_SystemCreateTimeUtc, OW_SystemCreateUser, OW_SystemLastEditTimeUtc, OW_SystemLastEditUser) VALUES (@pk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateOrgTradeProspect

		public Guid CreateOrgTradeProspect(Guid orgSalesPK, string packType)
		{
			Guid pk = Guid.NewGuid();
			Guid orgTradeDetailPk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateOrgTradeDetailSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@orgSalesPK", SqlDbType.UniqueIdentifier, orgSalesPK);
				command.AddParameter("@orgTradeDetailPk", SqlDbType.UniqueIdentifier, orgTradeDetailPk);
				command.AddParameter("@packType", SqlDbType.VarChar, OrgTradeProspectSchema.PAP_F3_NKPackType.MaxLength, packType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateOrgTradeDetailSql = @"
INSERT INTO dbo.OrgTradeDetail(PA_PK, PA_OW, PA_SystemCreateTimeUtc, PA_SystemCreateUser, PA_SystemLastEditTimeUtc, PA_SystemLastEditUser)
VALUES (@orgTradeDetailPk, @orgSalesPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');
INSERT INTO dbo.OrgTradeProspect(PAP_PK, PAP_PA, PAP_F3_NKPackType, PAP_SystemCreateTimeUtc, PAP_SystemCreateUser, PAP_SystemLastEditTimeUtc, PAP_SystemLastEditUser)
VALUES (@pk, @orgTradeDetailPk, @packType, GetUtcDate(), '~BP', GetUtcDate(), '~BP');";

		#endregion

		#region CreatePEHeader

		public Guid CreatePEHeader(Guid companyPK, string jobNumber, string packType, string user)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreatePEHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@jobNumber", SqlDbType.VarChar, 20, jobNumber);
				command.AddParameter("@packType", SqlDbType.VarChar, 3, packType);
				command.AddParameter("@createTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.AddParameter("@editTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.AddParameter("@user", SqlDbType.VarChar, 3, user);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreatePEHeaderSql = @"INSERT INTO PEHeader(PEJ_PK, PEJ_GC_Company, PEJ_JobNumber, PEJ_F3_NKPackType, PEJ_SystemCreateTimeUtc, PEJ_SystemLastEditTimeUtc, PEJ_SystemCreateUser, PEJ_SystemLastEditUser) VALUES (@pk, @companyPK, @jobNumber, @packType, @createTimeUtc, @editTimeUtc, @user, @user)";

		#endregion

		#region CreatePELine

		public Guid CreatePELine(Guid headerPK, string packType, string user)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreatePELineSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@headerPK", SqlDbType.UniqueIdentifier, headerPK);
				command.AddParameter("@packType", SqlDbType.VarChar, 3, packType);
				command.AddParameter("@createTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.AddParameter("@editTimeUtc", SqlDbType.DateTime, DateTime.Now);
				command.AddParameter("@user", SqlDbType.VarChar, 3, user);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreatePELineSql = @"INSERT INTO PELine(PEL_PK, PEL_PEJ_Header, PEL_F3_NKPackType, PEL_SystemCreateTimeUtc, PEL_SystemLastEditTimeUtc, PEL_SystemCreateUser, PEL_SystemLastEditUser) VALUES (@pk, @headerPK, @packType, @createTimeUtc, @editTimeUtc, @user, @user)";

		#endregion

		#region CreateOrgSupplierPartBarcode

		public Guid CreateOrgSupplierPartBarcode(string barcode, Guid productPK, string packType)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateOrgSupplierPartBarcodeSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@barcode", SqlDbType.VarChar, OrgSupplierPartBarcodeSchema.PH_Barcode.MaxLength, barcode);
				command.AddParameter("@productPK", SqlDbType.UniqueIdentifier, productPK);
				command.AddParameter("@packType", SqlDbType.VarChar, OrgSupplierPartBarcodeSchema.PH_F3_NKPackType.MaxLength, packType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateOrgSupplierPartBarcodeSql = @"
INSERT INTO dbo.OrgSupplierPartBarcode(PH_PK, PH_Barcode, PH_OP, PH_F3_NKPackType, PH_SystemCreateTimeUtc, PH_SystemCreateUser, PH_SystemLastEditTimeUtc, PH_SystemLastEditUser)
VALUES (@pk, @barcode, @productPK, @packType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateOrgSupplierBuyerLink

		public Guid CreateOrgSupplierBuyerLink(Guid buyerPK, Guid supplierPK)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateOrgSupplierBuyerLinkSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@buyerPK", SqlDbType.UniqueIdentifier, buyerPK);
				command.AddParameter("@supplierPK", SqlDbType.UniqueIdentifier, supplierPK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateOrgSupplierBuyerLinkSql = @"
INSERT INTO dbo.OrgSupplierBuyerLink(OL_PK, OL_OH_Supplier, OL_OH_Buyer, OL_SystemCreateTimeUtc, OL_SystemCreateUser, OL_SystemLastEditTimeUtc, OL_SystemLastEditUser)
VALUES (@pk, @supplierPK, @buyerPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateOrgBuyerSupplierLinkPackPivot

		public Guid CreateOrgBuyerSupplierLinkPackPivot(Guid q0OLPK, Guid packTypePK)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateOrgBuyerSupplierLinkPackPivotSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@q0OLPK", SqlDbType.UniqueIdentifier, q0OLPK);
				command.AddParameter("@packTypePK", SqlDbType.UniqueIdentifier, packTypePK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateOrgBuyerSupplierLinkPackPivotSql = @"
INSERT INTO dbo.OrgBuyerSupplierLinkPackPivot(Q0_PK, Q0_OL, Q0_F3, Q0_SystemCreateTimeUtc, Q0_SystemCreateUser, Q0_SystemLastEditTimeUtc, Q0_SystemLastEditUser)
VALUES (@pk, @q0OLPK, @packTypePK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateRefEquipment

		public Guid CreateRefEquipment(string shortCode, string registration, string packType)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateRefEquipmentSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@shortCode", SqlDbType.VarChar, RefEquipmentSchema.RQ_ShortCode.MaxLength, shortCode);
				command.AddParameter("@registration", SqlDbType.VarChar, RefEquipmentSchema.RQ_Registration.MaxLength, registration);
				command.AddParameter("@packType", SqlDbType.VarChar, RefEquipmentSchema.RQ_F3_NKPackType.MaxLength, packType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateRefEquipmentSql = @"
INSERT INTO dbo.RefEquipment(RQ_PK, RQ_ShortCode, RQ_Registration, RQ_F3_NKPackType, RQ_SystemCreateTimeUtc, RQ_SystemCreateUser, RQ_SystemLastEditTimeUtc, RQ_SystemLastEditUser)
VALUES (@pk, @shortCode, @registration, @packType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateRateOneOffContainers

		public Guid CreateRateOneOffContainers(string packType, Guid? oneOffShipmentPK = null)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRateOneOffContainersSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				if (oneOffShipmentPK != null)
				{
					command.AddParameter("@oneOffShipmentPK", SqlDbType.UniqueIdentifier, oneOffShipmentPK.Value);
				}
				else
				{
					command.AddParameter("@oneOffShipmentPK", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateRateOneOffContainersSql = @"
INSERT INTO dbo.RateOneOffContainers(TC_PK, TC_F3_NKPackType, TC_TT, TC_SystemCreateTimeUtc, TC_SystemCreateUser, TC_SystemLastEditTimeUtc, TC_SystemLastEditUser)
VALUES (@pk, @packType, @oneOffShipmentPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreatePortHubSelection

		public Guid CreatePortHubSelection(Guid depotAddressPK, string packType)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreatePortHubSelectionSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@depotAddressPK", SqlDbType.UniqueIdentifier, depotAddressPK);
				command.AddParameter("@packType", SqlDbType.VarChar, PortHubSelectionSchema.TY_F3_NKPackType.MaxLength, packType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreatePortHubSelectionSql = @"
INSERT INTO dbo.PortHubSelection(TY_PK, TY_OA_DepotAddress, TY_F3_NKPackType, TY_SystemCreateTimeUtc, TY_SystemCreateUser, TY_SystemLastEditTimeUtc, TY_SystemLastEditUser)
VALUES (@pk, @depotAddressPK, @packType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		#endregion

		#region CreateJobMawb

		const string CreateJobMawbSql = @"
INSERT INTO dbo.JobMawb (JM_PK, JM_Airline3DigitPrefix, JM_MAWB, JM_SystemCreateTimeUtc, JM_SystemCreateUser, JM_SystemLastEditTimeUtc, JM_SystemLastEditUser)
VALUES (@pk, @airline3DigitPrefix, @mawb, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobMawb(string airline3DigitPrefix, string mawb)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobMawbSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@airline3DigitPrefix", SqlDbType.VarChar, airline3DigitPrefix);
				command.AddParameter("@mawb", SqlDbType.VarChar, mawb);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobvoyage

		const string CreateJobVoyageSql = @"INSERT INTO dbo.JobVoyage (JV_PK, JV_RV_NKVessel, JV_VoyageFlight, JV_IsChartered, JV_IsCargoOnly, JV_AircraftType, JV_SendersMessageReference, JV_SystemCreateTimeUtc, JV_SystemCreateUser, JV_SystemLastEditTimeUtc, JV_SystemLastEditUser)
											VALUES (@pk, @vessel, @voyageFlight, @isChartered, @isCargoOnly, @aircraftType, @messageReference, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobVoyage(string vessel, string voyageFlight, bool isCharter, string airCraftType, string messageReference = "", bool isCargoOnly = false)
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateJobVoyageSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@vessel", SqlDbType.VarChar, vessel);
				command.AddParameter("@voyageFlight", SqlDbType.VarChar, voyageFlight);
				command.AddParameter("@isChartered", SqlDbType.Bit, isCharter);
				command.AddParameter("@isCargoOnly", SqlDbType.Bit, isCargoOnly);
				command.AddParameter("@aircraftType", SqlDbType.VarChar, airCraftType);
				command.AddParameter("@messageReference", SqlDbType.VarChar, messageReference);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobVoyOrigin

		const string CreateJobVoyOriginSql = @"INSERT INTO dbo.JobVoyOrigin (JA_PK, JA_JV, JA_RL_NKPortOfLoading, JA_OA_DepartureCTOAddress, JA_DepartReference, JA_S_DEP, JA_E_DEP, JA_A_DEP, JA_SendersMessageReference, JA_SystemCreateTimeUtc, JA_SystemCreateUser, JA_SystemLastEditTimeUtc, JA_SystemLastEditUser)
											VALUES (@pk, @jobVoyagePK, @portOfLoading, @departureAddressPK, @departReference, @std, @etd, @atd, @messageReference, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobVoyOrigin(Guid voyagePk, string portOfLoading, Guid? departureCTOAddress, string departReference, DateTime std, DateTime etd, DateTime atd, string messageReference = "")
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateJobVoyOriginSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobVoyagePK", SqlDbType.UniqueIdentifier, voyagePk);
				command.AddParameter("@portOfLoading", SqlDbType.VarChar, portOfLoading);
				command.AddParameter("@departureAddressPK", SqlDbType.UniqueIdentifier, departureCTOAddress ?? (object)DBNull.Value);
				command.AddParameter("@departReference", SqlDbType.VarChar, departReference);
				command.AddParameter("@messageReference", SqlDbType.VarChar, messageReference);
				command.AddParameter("@std", SqlDbType.DateTime, std);
				command.AddParameter("@etd", SqlDbType.DateTime, etd);
				command.AddParameter("@atd", SqlDbType.DateTime, atd);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobVoyDestination

		const string CreateJobVoyDestinationSql = @"INSERT INTO dbo.JobVoyDestination (JB_PK, JB_JV, JB_RL_NKPortOfDischarge, JB_OA_ArrivalCTOAddress, JB_ArrivalReference, JB_S_ARV, JB_E_ARV, JB_A_ARV, JB_SendersMessageReference, JB_SystemCreateTimeUtc, JB_SystemCreateUser, JB_SystemLastEditTimeUtc, JB_SystemLastEditUser)
													VALUES (@pk, @jobVoyagePK, @portOfDischarge, @arrivalAddressPK, @arrivalReference, @sta, @eta, @ata, @messageReference, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobVoyDestination(Guid voyagePk, string portOfDischarge, Guid? arrivalCTOAddress, string arrivalReference, DateTime sta, DateTime eta, DateTime ata, string messageReference = "")
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateJobVoyDestinationSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobVoyagePK", SqlDbType.UniqueIdentifier, voyagePk);
				command.AddParameter("@portOfDischarge", SqlDbType.VarChar, portOfDischarge);
				command.AddParameter("@arrivalAddressPK", SqlDbType.UniqueIdentifier, arrivalCTOAddress ?? (object)DBNull.Value);
				command.AddParameter("@arrivalReference", SqlDbType.VarChar, arrivalReference);
				command.AddParameter("@messageReference", SqlDbType.VarChar, messageReference);
				command.AddParameter("@sta", SqlDbType.DateTime, sta);
				command.AddParameter("@eta", SqlDbType.DateTime, eta);
				command.AddParameter("@ata", SqlDbType.DateTime, ata);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateSailing

		const string CreateSailingSql = @"
INSERT INTO dbo.JobSailing (JX_PK, JX_JA, JX_JB, JX_ArrivalPortRouteId, JX_DeparturePortRouteId, JX_UniqueReference, JX_SystemCreateTimeUtc, JX_SystemCreateUser, JX_SystemLastEditTimeUtc, JX_SystemLastEditUser)
VALUES (@pk, @originPK, @destinationPK, @arrivalPortRouteId, @departurePortRouteId, @sailingReference, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateSailing(Guid originPk, Guid destinationPk, string arrivalPortRouteId, string departurePortRouteId, string sailingReference = "")
		{
			Guid pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateSailingSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@originPK", SqlDbType.UniqueIdentifier, originPk);
				command.AddParameter("@destinationPK", SqlDbType.UniqueIdentifier, destinationPk);
				command.AddParameter("@arrivalPortRouteId", SqlDbType.VarChar, arrivalPortRouteId);
				command.AddParameter("@departurePortRouteId", SqlDbType.VarChar, departurePortRouteId);
				command.AddParameter("@sailingReference", SqlDbType.VarChar, sailingReference);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefContainerStock

		const string CreateRefContainerStockSql = @"INSERT INTO dbo.RefContainerStock (R6_PK, R6_RC, R6_SystemCreateTimeUtc, R6_SystemCreateUser, R6_SystemLastEditTimeUtc, R6_SystemLastEditUser) VALUES (@pk, @containerPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefContainerStock(Guid containerPk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefContainerStockSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@containerPk", SqlDbType.UniqueIdentifier, containerPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobContainerMove

		const string CreateJobContainerMoveSql = @"INSERT INTO dbo.JobContainerMove (E9_PK, E9_JV, E9_R6, E9_SystemCreateTimeUtc, E9_SystemCreateUser, E9_SystemLastEditTimeUtc, E9_SystemLastEditUser) VALUES (@pk, @jobVoyagePK, @containerStockPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobContainerMove(Guid voyagePk, Guid containerStockPk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobContainerMoveSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobVoyagePK", SqlDbType.UniqueIdentifier, voyagePk);
				command.AddParameter("@containerStockPk", SqlDbType.UniqueIdentifier, containerStockPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobTradeLaneVoyage

		const string CreateJobTradeLaneVoyageSql = @"INSERT INTO dbo.JobTradeLaneVoyage (NB_PK, NB_JV, NB_OH, NB_SystemCreateTimeUtc, NB_SystemCreateUser, NB_SystemLastEditTimeUtc, NB_SystemLastEditUser) VALUES (@pk, @jobVoyagePK, @orgPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobTradeLaneVoyage(Guid voyagePk, Guid orgPk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobTradeLaneVoyageSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobVoyagePK", SqlDbType.UniqueIdentifier, voyagePk);
				command.AddParameter("@orgPk", SqlDbType.UniqueIdentifier, orgPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobVoyAccount

		const string CreateJobVoyAccountSql = @"INSERT INTO dbo.JobVoyAccount (NA_PK, NA_JV, NA_OH, NA_GC, NA_SystemCreateTimeUtc, NA_SystemCreateUser, NA_SystemLastEditTimeUtc, NA_SystemLastEditUser) VALUES (@pk, @jobVoyagePK, @orgPk, @companyPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobVoyAccount(Guid voyagePk, Guid orgPk, Guid companyPk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobVoyAccountSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobVoyagePK", SqlDbType.UniqueIdentifier, voyagePk);
				command.AddParameter("@orgPk", SqlDbType.UniqueIdentifier, orgPk);
				command.AddParameter("@companyPk", SqlDbType.UniqueIdentifier, companyPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobVoyageExRate

		const string CreateJobVoyageExRateSql = @"INSERT INTO dbo.JobVoyageExRate (E8_PK, E8_JV, E8_GC, E8_SystemCreateTimeUtc, E8_SystemCreateUser, E8_SystemLastEditTimeUtc, E8_SystemLastEditUser) VALUES (@pk, @jobVoyagePK, @companyPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobVoyageExRate(Guid voyagePk, Guid companyPk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobVoyageExRateSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobVoyagePK", SqlDbType.UniqueIdentifier, voyagePk);
				command.AddParameter("@companyPk", SqlDbType.UniqueIdentifier, companyPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobVoyCountry

		const string CreateJobVoyCountrySql = @"INSERT INTO dbo.JobVoyCountry (J0_PK, J0_JV, J0_SystemCreateTimeUtc, J0_SystemCreateUser, J0_SystemLastEditTimeUtc, J0_SystemLastEditUser) VALUES (@pk, @jobVoyagePK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobVoyCountry(Guid voyagePk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobVoyCountrySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobVoyagePK", SqlDbType.UniqueIdentifier, voyagePk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobComInvoiceHeader

		const string CreateJobComInvoiceHeaderSql = @"INSERT INTO dbo.JobComInvoiceHeader(JZ_PK, JZ_JE, JZ_GB, JZ_AddInfo, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser, JZ_DataModel) values(@invoiceHeaderPK, @declarationPK, @branchPK, @addInfo, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @dataModel)";

		public Guid CreateJobComInvoiceHeader(Guid? branchPK, Guid? declarationPK, int clusterKey, string addInfo = "", string dataModel = "")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobComInvoiceHeaderSql))
			{
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK ?? (object)DBNull.Value);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK ?? (object)DBNull.Value);
				command.AddParameter("@addInfo", SqlDbType.NVarChar, addInfo);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateCusAddInfo

		const string CreateCusAddInfoSql = @"
INSERT INTO dbo.CusAddInfo(B7_PK, B7_Type, B7_ParentTableCode, B7_ParentID, B7_AddInfoData, B7_SystemCreateTimeUtc, B7_SystemCreateUser, B7_SystemLastEditTimeUtc, B7_SystemLastEditUser)
VALUES (@cusAddInfoPk, @type, 'JI', @linePk, @addInfoData, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateCusAddInfo(Guid cusAddInfoPk, Guid linePk, string type, string addInfoData)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateCusAddInfoSql))
			{
				command.AddParameter("@cusAddInfoPk", SqlDbType.UniqueIdentifier, cusAddInfoPk);
				command.AddParameter("@type", SqlDbType.VarChar, type);
				command.AddParameter("@linePk", SqlDbType.UniqueIdentifier, linePk);
				command.AddParameter("@addInfoData", SqlDbType.VarChar, addInfoData);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateCusAddInfoSql2 = @"
INSERT INTO dbo.CusAddInfo(B7_PK, B7_Type, B7_ParentTableCode, B7_ParentID, B7_AddInfoData, B7_SystemCreateTimeUtc, B7_SystemCreateUser, B7_SystemLastEditTimeUtc, B7_SystemLastEditUser)
VALUES (@pk, @type, @parentTableCode, @parentID, @addInfoData, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateCusAddInfo(string type, string addInfoData, Guid parentID, string parentTableCode)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateCusAddInfoSql2))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@type", SqlDbType.VarChar, CusAddInfoSchema.B7_Type.MaxLength, type);
				command.AddParameter("@addInfoData", SqlDbType.VarChar, CusAddInfoSchema.B7_AddInfoData.MaxLength, addInfoData);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, CusAddInfoSchema.B7_ParentTableCode.MaxLength, parentTableCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobComInvoiceLine

		const string CreateJobComInvoiceLineSql = @"INSERT INTO dbo.JobComInvoiceLine(JI_PK, JI_JZ, JI_BrandName, JI_DataModel, JI_ClusterKey, JI_AddInfo, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser)
				VALUES (@invoiceLinePK, @invoiceHeaderPK, @brandName, @dataModel, @clusterKey, @addInfo, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobComInvoiceLine(Guid headerPk, int clusterKey, string brandName = "", string dataModel = "", string addInfo = "")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobComInvoiceLineSql))
			{
				command.AddParameter("@invoiceLinePK", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@invoiceHeaderPK", SqlDbType.UniqueIdentifier, headerPk);
				command.AddParameter("@brandName", SqlDbType.VarChar, brandName);
				command.AddParameter("@addInfo", SqlDbType.NVarChar, addInfo);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateEDIMessage

		const string CreateEdiMessageSql = @"INSERT INTO dbo.EdiMessage (EM_PK, EM_GB, EM_GE, EM_ReceiveTransmit, EM_Status, EM_ApplicationCode, EM_ApplicationReference, EM_MessageOwner, EM_MessageType, EM_MessageSubType, EM_MessageNum, EM_LinkUniqueID, EM_LinkTable, EM_MessageText, EM_SystemCreateTimeUtc, EM_SystemCreateUser, EM_SystemLastEditTimeUtc, EM_SystemLastEditUser) VALUES (@pk, @branch, @department, @receiveTransmit, @status, @applicationCode, @applicationReference, @messageOwner, @messageType, @messageSubType, @messageNum, @linkUniqueID, @linkTable, @messageText, @systemCreateTime, '~BP', GetUtcDate(), '~BP')";

		public Guid CreateEDIMessage(Guid branch, Guid department, string receiveTransmit, string applicationCode, string status, string messageType, string messageNum, Guid linkUniqueID, string linkTable, string messageText, DateTime systemCreateTime, string messageOwner = "", string messageSubType = "", string applicationReference = "")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateEdiMessageSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branch);
				command.AddParameter("@department", SqlDbType.UniqueIdentifier, department);
				command.AddParameter("@receiveTransmit", SqlDbType.VarChar, receiveTransmit);
				command.AddParameter("@status", SqlDbType.VarChar, status);
				command.AddParameter("@applicationCode", SqlDbType.VarChar, applicationCode);
				command.AddParameter("@applicationReference", SqlDbType.VarChar, applicationReference);
				command.AddParameter("@messageOwner", SqlDbType.VarChar, messageOwner);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@messageSubType", SqlDbType.VarChar, messageSubType);
				command.AddParameter("@messageNum", SqlDbType.VarChar, messageNum);
				if (linkUniqueID == Guid.Empty)
				{
					command.AddParameter("@linkTable", SqlDbType.VarChar, string.Empty);
					command.AddParameter("@linkUniqueID", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
				else
				{
					command.AddParameter("@linkTable", SqlDbType.VarChar, linkTable);
					command.AddParameter("@linkUniqueID", SqlDbType.UniqueIdentifier, linkUniqueID);
				}
				command.AddParameter("@messageText", SqlDbType.VarChar, messageText);
				command.AddParameter("@systemCreateTime", SqlDbType.DateTime, systemCreateTime);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateCusEntryHeader

		const string CreateEntryHeaderSql = @"INSERT INTO dbo.CusEntryHeader (CH_PK, CH_BGMReference, CH_Status, CH_JE, CH_MessageType, CH_EntrySubmittedDate, CH_EntryReleaseDate, CH_ClusterKey, CH_DataModel, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser) VALUES (@pk, @bgmReference, @status, @jePK, @messageType, @entrySubmittedDate, @entryReleaseDate, @clusterKey, @dataModel, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateCusEntryHeader(string bgmReference, string status, Guid jePK, string messageType, DateTime entrySubmittedDate, DateTime entryReleaseDate, int clusterKey, string dataModel)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateEntryHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@bgmReference", SqlDbType.VarChar, bgmReference);
				command.AddParameter("@status", SqlDbType.VarChar, status);
				command.AddParameter("@jePK", SqlDbType.UniqueIdentifier, jePK);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@entrySubmittedDate", SqlDbType.DateTime, entrySubmittedDate);
				command.AddParameter("@entryReleaseDate", SqlDbType.DateTime, entryReleaseDate);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateCusEntryLine

		const string CreateEntryLineSql = @"INSERT INTO dbo.CusEntryLine (CL_PK, CL_CH, CL_ClusterKey, CL_DataModel, CL_SystemCreateTimeUtc, CL_SystemCreateUser, CL_SystemLastEditTimeUtc, CL_SystemLastEditUser) VALUES (@pk, @headerPk, @clusterKey, @dataModel, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateCusEntryLine(Guid headerPk, int clusterKey, string dataModel)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateEntryLineSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@headerPk", SqlDbType.UniqueIdentifier, headerPk);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateCusEntryNum

		const string CreateEntryNumSql = @"INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_EntryType, CE_Category, CE_RN_NKCountryCode, CE_IssueDate, CE_ExpiryDate, CE_EntryIsSystemGenerated, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES (@pk, @parentPk, @parentTable, @entryNum, @entryType, @category, @countryCode, @issueDate, @expiryDate, @isSystemGenerated, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public void CreateCusEntryNum(Guid pk, Guid parentPk, string parentTable, string entryNum, string entryType, string countryCode = "", string category = "CUS", DateTime? issueDate = null, DateTime? expiryDate = null, bool isSystemGenerated = false)
		{
			using (var command = Db.Connection.Command(CreateEntryNumSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentPk", SqlDbType.UniqueIdentifier, parentPk);
				command.AddParameter("@parentTable", SqlDbType.VarChar, parentTable);
				command.AddParameter("@entryNum", SqlDbType.VarChar, entryNum);
				command.AddParameter("@entryType", SqlDbType.VarChar, entryType);
				command.AddParameter("@issueDate", SqlDbType.DateTime, issueDate == null ? DBNull.Value : issueDate);
				command.AddParameter("@expiryDate", SqlDbType.DateTime, expiryDate == null ? DBNull.Value : expiryDate);
				command.AddParameter("@countryCode", SqlDbType.VarChar, countryCode);
				command.AddParameter("@category", SqlDbType.VarChar, category);
				command.AddParameter("@isSystemGenerated", SqlDbType.Bit, isSystemGenerated);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region CreateCusISFHeader

		const string CreateCusISFHeaderSql = @"
INSERT INTO [dbo].[CusISFHeader] ([BF_PK], [BF_JobReference], [BF_GB], [BF_SystemCreateTimeUtc], [BF_SystemCreateUser], [BF_SystemLastEditTimeUtc], [BF_SystemLastEditUser])
VALUES (@pk, @jobReference, @glbBranchPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateCusISFHeader(string jobReference, Guid glbBranchPK)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateCusISFHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobReference", SqlDbType.VarChar, jobReference);
				command.AddParameter("@glbBranchPK", SqlDbType.UniqueIdentifier, glbBranchPK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobDeclaration

		const string CreateJobDeclarationSql = @"INSERT INTO dbo.JobDeclaration (JE_PK, JE_DataModel, JE_GB, JE_GC, JE_OH_Supplier, JE_MessageType, JE_SystemCreateTimeUtc, JE_ClusterKey, JE_DeclarationReference, JE_MessageStatus, JE_CarrierCode, JE_AddInfo, JE_SystemCreateUser, JE_SystemLastEditTimeUtc, JE_SystemLastEditUser) VALUES (@pk, @dataModel, @branch, @company, @supplier, @messageType, @createdTime, @clusterKey, @decRef, @messageStatus, @carrierCode, @addInfo, '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobDeclaration(string dataModel, Guid branch, Guid company, Guid supplier, string messageType, DateTime createdTime, int clusterKey, string messageStatus, string carrierCode = "", string addInfo = "")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobDeclarationSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@dataModel", SqlDbType.VarChar, dataModel);
				command.AddParameter("@branch", SqlDbType.UniqueIdentifier, branch);
				command.AddParameter("@company", SqlDbType.UniqueIdentifier, company);
				command.AddParameter("@supplier", SqlDbType.UniqueIdentifier, supplier);
				command.AddParameter("@messageType", SqlDbType.VarChar, messageType);
				command.AddParameter("@createdTime", SqlDbType.DateTime, createdTime);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.AddParameter("@decRef", SqlDbType.VarChar, clusterKey);
				command.AddParameter("@messageStatus", SqlDbType.VarChar, messageStatus);
				command.AddParameter("@carrierCode", SqlDbType.VarChar, carrierCode);
				command.AddParameter("@addInfo", SqlDbType.VarChar, addInfo);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateStandaloneJobComInvoiceHeader

		const string CreateStandaloneJobComInvoiceHeaderSql = @"INSERT INTO dbo.JobComInvoiceHeader (JZ_PK, JZ_ClusterKey, JZ_SystemCreateTimeUtc, JZ_SystemCreateUser, JZ_SystemLastEditTimeUtc, JZ_SystemLastEditUser) VALUES (@pk, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateStandaloneJobComInvoiceHeader(int clusterKey)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateStandaloneJobComInvoiceHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobComInvoiceLine

		const string CreateInvoiceLineSql = @"INSERT INTO dbo.JobComInvoiceLine (JI_PK, JI_JZ, JI_CL, JI_ClusterKey, JI_SystemCreateTimeUtc, JI_SystemCreateUser, JI_SystemLastEditTimeUtc, JI_SystemLastEditUser) VALUES (@pk, @jz, @entryLine, @clusterKey, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateInvoiceLine(Guid jz, Guid entryLine, int clusterKey)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateInvoiceLineSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jz", SqlDbType.UniqueIdentifier, jz);
				command.AddParameter("@entryLine", SqlDbType.UniqueIdentifier, entryLine);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGlbCompany

		const string CreateGlbCompanySql = @"
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
VALUES (@pk, @code, 'AU company', @countryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		const string CreateGlbCompanySql1 = @"
INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_OH_OrgProxy, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)
VALUES (@pk, @code, 'AU company', @countryCode, @orgProxy, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGlbCompany(string code, string countryCode)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbCompanySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, code);
				command.AddParameter("@countryCode", SqlDbType.VarChar, countryCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid CreateGlbCompany(string code, string countryCode, Guid orgProxy)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbCompanySql1))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, code);
				command.AddParameter("@orgProxy", SqlDbType.UniqueIdentifier, orgProxy);
				command.AddParameter("@countryCode", SqlDbType.VarChar, countryCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGlbBranch

		const string CreateGlbBranchSql = @"
INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser)
VALUES (@pk, @code, @company, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGlbBranch(string code, Guid company)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbBranchSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, code);
				command.AddParameter("@company", SqlDbType.UniqueIdentifier, company);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGlbGroup

		const string CreateGlbGroupSql = @"
INSERT INTO dbo.GlbGroup (GG_PK, GG_Code, GG_Type, GG_Desc, GG_SystemCreateTimeUtc, GG_SystemCreateUser, GG_SystemLastEditTimeUtc, GG_SystemLastEditUser)
VALUES (@pk, @code, @type, @desc, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGlbGroup(string code, string type, string desc = "")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbGroupSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, code);
				command.AddParameter("@type", SqlDbType.VarChar, type);
				command.AddParameter("@desc", SqlDbType.VarChar, desc);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string MarkGlbGroupAsSystemDefinedSql = @"
UPDATE dbo.GlbGroup
SET GG_IsSystemDefined = @isSystemDefined, GG_SystemLastEditTimeUtc = GetUtcDate(), GG_SystemLastEditUser = '~BP'
WHERE GG_PK = @pk";

		public void MarkGlbGroupAsSystemDefined(Guid groupPk, bool isSystemDefined)
		{
			using var command = Db.Connection.Command(MarkGlbGroupAsSystemDefinedSql);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, groupPk);
			command.AddParameter("@isSystemDefined", SqlDbType.Bit, isSystemDefined);
			var rowAffected = command.ExecuteNonQuery();
			if (rowAffected == 0)
			{
				throw new ArgumentException($"No record found with Group PK {groupPk}");
			}
		}

		public Guid GetOrCreateGroupByCode(string code, string type, string desc, bool isSystemDefined)
		{
			var groupPk = (Guid?)GetNullableValueFromTableByValue("GG_PK", "dbo.GlbGroup", "GG_Code", code) ??
				CreateGlbGroup(code, type, desc);
			MarkGlbGroupAsSystemDefined(groupPk, isSystemDefined);
			return groupPk;
		}

		object GetNullableValueFromTableByValue(string selectColumn, string table, string valueColumn, string value)
		{
			return Db.Connection.ExecuteScalar("SELECT " + selectColumn + " FROM " + table + " WHERE " + valueColumn + " = '" + value + "'");
		}

		#endregion

		#region CreateGlbGroupRole

		const string CreateGlbGroupRoleSql = @"
INSERT INTO dbo.GlbGroupRole(GGR_PK, GGR_RoleName, GGR_GG_Group, GGR_AutoVersion, GGR_SystemCreateTimeUtc, GGR_SystemCreateUser, GGR_SystemLastEditTimeUtc, GGR_SystemLastEditUser)
VALUES (@pk, @roleName, @groupPK, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGlbGroupRole(Guid groupPK, string roleName)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbGroupRoleSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@roleName", SqlDbType.VarChar, roleName);
				command.AddParameter("@groupPK", SqlDbType.UniqueIdentifier, groupPK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region GlbSecurity

		const string CreateGlbSecuritySql = @"
INSERT INTO dbo.GlbSecurity (GU_PK, GU_SecurityRight, GU_GG, GU_SystemCreateTimeUtc, GU_SystemCreateUser, GU_SystemLastEditTimeUtc, GU_SystemLastEditUser)
VALUES (@pk, @securityRight, @groupPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP');";

		public Guid CreateGlbSecurity(string securityRight, Guid groupPK)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbSecuritySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@securityRight", SqlDbType.VarChar, securityRight);
				command.AddParameter("@groupPK", SqlDbType.UniqueIdentifier, groupPK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGlbGroupLink

		const string CreateGlbGroupLinkSql = @"
INSERT INTO dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS, GK_SystemCreateTimeUtc, GK_SystemCreateUser, GK_SystemLastEditTimeUtc, GK_SystemLastEditUser)
VALUES (@pk, @groupPk, @staffPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGlbGroupLink(Guid groupPk, Guid staffPk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbGroupLinkSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@groupPk", SqlDbType.UniqueIdentifier, groupPk);
				command.AddParameter("@staffPk", SqlDbType.UniqueIdentifier, staffPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid GetOrCreateGlbGroupRole(Guid groupPk, string roleName)
		{
			return Db.Connection.Exists($"FROM dbo.GlbGroupRole WHERE GGR_GG_Group = '{groupPk}' AND GGR_RoleName = '{roleName}'")
				? Db.Connection.ExecuteScalar<Guid>($"SELECT GGR_PK FROM dbo.GlbGroupRole WHERE GGR_GG_Group = '{groupPk}' AND GGR_RoleName = '{roleName}'")
				: CreateGlbGroupRole(groupPk, roleName);
		}

		#endregion

		#region CreateGlbGroupRole

		const string GlbGroupRole = @"
INSERT INTO dbo.GlbGroupRole (GGR_PK , GGR_RoleName, GGR_GG_Group, GGR_SystemCreateTimeUtc, GGR_SystemCreateUser, GGR_SystemLastEditTimeUtc, GGR_SystemLastEditUser)
VALUES (@pk, @roleName, @groupId, GetUtcDate(), 'E', GetUtcDate(), 'E')";

		public Guid CreateGlbGroupRole(string roleName, Guid groupId)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(GlbGroupRole))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@roleName", SqlDbType.VarChar, roleName);
				command.AddParameter("@groupId", SqlDbType.UniqueIdentifier, groupId);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGlbDepartment

		const string CreateGlbDepartmentSql = @"
INSERT INTO dbo.GlbDepartment (GE_PK, GE_Code, GE_SystemCreateTimeUtc, GE_SystemCreateUser, GE_SystemLastEditTimeUtc, GE_SystemLastEditUser)
VALUES (@pk, @code, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGlbDepartment(string code)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbDepartmentSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, code);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrg

		const string CreateOrgHeaderSql = @"INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) VALUES (@pk, @code, @fullName, @conutryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrg(string code, string fullName = "", string conutryCode = "")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, code);
				command.AddParameter("@fullName", SqlDbType.VarChar, fullName);
				command.AddParameter("@conutryCode", SqlDbType.VarChar, conutryCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgAddress

		const string CreateOrgAddressSql = @"INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1, OA_Code, OA_CompanyNameOverride, OA_RN_NKCountryCode, OA_RL_NKRelatedPortCode, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser) VALUES (@pk, @oh, @address1, @code, @companyOverride, @countryCode, @portCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgAddress(Guid oh, string address1, string code, string companyOverride, string countryCode = "", string portCode = "")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgAddressSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@oh", SqlDbType.UniqueIdentifier, oh);
				command.AddParameter("@address1", SqlDbType.VarChar, address1);
				command.AddParameter("@code", SqlDbType.VarChar, code);
				command.AddParameter("@companyOverride", SqlDbType.VarChar, companyOverride);
				command.AddParameter("@countryCode", SqlDbType.VarChar, countryCode);
				command.AddParameter("@portCode", SqlDbType.VarChar, portCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgAddressCapability

		const string CreateAddressCapabilitySql = @"
INSERT INTO dbo.OrgAddressCapability(PZ_PK, PZ_IsValid, PZ_AddressType, PZ_OA, PZ_IsMainAddress, PZ_SystemCreateTimeUtc, PZ_SystemCreateUser, PZ_SystemLastEditTimeUtc, PZ_SystemLastEditUser)
VALUES (@pk, @isValid, @addressType, @oa, @isMainAddress, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgAddressCapability(int isValid, string addressType, Guid oa, int isMainAddress)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateAddressCapabilitySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@isValid", SqlDbType.Int, isValid);
				command.AddParameter("@addressType", SqlDbType.VarChar, addressType);
				command.AddParameter("@oa", SqlDbType.UniqueIdentifier, oa);
				command.AddParameter("@isMainAddress", SqlDbType.Int, isMainAddress);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgOpportunity

		const string CreateOrgOpportunitySql = @"INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OH, P8_OpportunityID, P8_GC, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser) VALUES (@pk, @orgPk, @opportunityId, @companyPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgOpportunity(string opportunityId, Guid orgPk, Guid companyPk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgOpportunitySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@orgPk", SqlDbType.UniqueIdentifier, orgPk);
				command.AddParameter("@opportunityId", SqlDbType.VarChar, opportunityId);
				command.AddParameter("@companyPk", SqlDbType.UniqueIdentifier, companyPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgOpportunityValue

		const string CreateOrgOpportunityValueSql = @"
INSERT INTO dbo.OrgOpportunityValue (PV_PK, PV_RevenueType, PV_Value, PV_DiscountBasis, PV_Discount, PV_DiscountPercent, PV_P8, PV_SystemCreateTimeUtc, PV_SystemCreateUser, PV_SystemLastEditTimeUtc, PV_SystemLastEditUser)
VALUES (@pk, @revenueType, @value, @discountBasis, @discount, @discountPercent, @opportunityPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgOpportunityValue(Guid opportunityPk, string revenueType = "OTH", decimal value = 1m, string discountBasis = "PCT", decimal discount = 2m, decimal discountPercent = 3m)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgOpportunityValueSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@revenueType", SqlDbType.VarChar, revenueType);
				command.AddParameter("@value", SqlDbType.Money, value);
				command.AddParameter("@discountBasis", SqlDbType.VarChar, discountBasis);
				command.AddParameter("@discount", SqlDbType.Money, discount);
				command.AddParameter("@discountPercent", SqlDbType.Decimal, discountPercent);
				command.AddParameter("@opportunityPk", SqlDbType.UniqueIdentifier, opportunityPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgCommissionAgreement

		const string CreateOrgCommissionAgreementSql = "INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_P8, CA0_Name, CA0_OH_Customer, CA0_CommissionBasis, CA0_CommissionTriggerType, CA0_CA0_ParentVersion, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser) VALUES (@pk, @oppPk, @name, @orgPk, 'PRF', 'CCD', @parentVersion, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgCommissionAgreement(Guid oppPk, string name, Guid orgPk, Guid parentVersion = default)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgCommissionAgreementSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@oppPk", SqlDbType.UniqueIdentifier, oppPk);
				command.AddParameter("@name", SqlDbType.VarChar, name);
				command.AddParameter("@orgPk", SqlDbType.UniqueIdentifier, orgPk);
				command.AddParameter("@parentVersion", SqlDbType.UniqueIdentifier, parentVersion == default ? DBNull.Value : parentVersion);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgCommissionAgreementItem

		const string CreateOrgCommissionAgreementItemSql = "INSERT INTO dbo.OrgCommissionAgreementItem (CAI_PK, CAI_ParentID, CAI_ParentTableCode, CAI_Type, CAI_Code, CAI_SystemCreateTimeUtc, CAI_SystemCreateUser, CAI_SystemLastEditTimeUtc, CAI_SystemLastEditUser) VALUES (@itemPk, @parentPk, @parentCode, 'PRD', @code, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgCommissionAgreementItem(Guid parentPk, string code, string parentCode)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgCommissionAgreementItemSql))
			{
				command.AddParameterBasedOnDbColumn("@itemPk", pk, OrgCommissionAgreementItemSchema.PK);
				command.AddParameterBasedOnDbColumn("@parentPk", parentPk, OrgCommissionAgreementItemSchema.CAI_ParentID);
				command.AddParameterBasedOnDbColumn("@parentCode", parentCode, OrgCommissionAgreementItemSchema.CAI_ParentTableCode);
				command.AddParameterBasedOnDbColumn("@code", code, OrgCommissionAgreementItemSchema.CAI_Code);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgCommissionAgreementItemCondition
		const string CreateOrgCommissionAgreementItemConditionSql = "INSERT INTO dbo.OrgCommissionAgreementItemCondition(CIC_PK, CIC_CAI, CIC_Mode, CIC_SystemCreateTimeUtc, CIC_SystemCreateUser, CIC_SystemLastEditTimeUtc, CIC_SystemLastEditUser) VALUES (@pk, @itemPk, @mode, GetUtcDate(), 'US1', GetUtcDate(), 'US1')";

		public Guid CreateOrgCommissionAgreementItemCondition(Guid caPk, string mode)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgCommissionAgreementItemConditionSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, OrgCommissionAgreementItemConditionSchema.PK);
				command.AddParameterBasedOnDbColumn("@itemPk", caPk, OrgCommissionAgreementItemConditionSchema.CIC_CAI);
				command.AddParameterBasedOnDbColumn("@mode", mode, OrgCommissionAgreementItemConditionSchema.CIC_Mode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgCommissionAgreementRecipient

		const string CreateOrgCommissionAgreementRecipientSql = @"INSERT INTO dbo.OrgCommissionAgreementRecipient(CAR_PK, CAR_CA0, CAR_GS_NKStaff, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser) VALUES (@pk, @orgCommissionAgreementPK, 'STF', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgCommissionAgreementRecipient(Guid orgCommissionAgreementPK)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgCommissionAgreementRecipientSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, OrgCommissionAgreementRecipientSchema.PK);
				command.AddParameterBasedOnDbColumn("@orgCommissionAgreementPK", orgCommissionAgreementPK, OrgCommissionAgreementRecipientSchema.PK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgCommissionAgreementRecipientRate

		const string CreateOrgCommissionAgreementRecipientRateSql = @"INSERT INTO dbo.OrgCommissionAgreementRecipientRate(CAT_PK, CAT_CAR, CAT_CommissionAmount, CAT_CommissionPercentage, CAT_SystemCreateTimeUtc, CAT_SystemCreateUser, CAT_SystemLastEditTimeUtc, CAT_SystemLastEditUser) VALUES (@pk, @commissionAgreementRecipientPK, @commissionAmount, @commissionPercentage, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgCommissionAgreementRecipientRate(Guid commissionAgreementRecipientPK, decimal commissionAmount = 0, decimal commissionPercentage = 0)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgCommissionAgreementRecipientRateSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, OrgCommissionAgreementRecipientRateSchema.PK);
				command.AddParameterBasedOnDbColumn("@commissionAgreementRecipientPK", commissionAgreementRecipientPK, OrgCommissionAgreementRecipientRateSchema.CAT_CAR);
				command.AddParameterBasedOnDbColumn("@commissionAmount", commissionAmount, OrgCommissionAgreementRecipientRateSchema.CAT_CommissionAmount);
				command.AddParameterBasedOnDbColumn("@commissionPercentage", commissionPercentage, OrgCommissionAgreementRecipientRateSchema.CAT_CommissionPercentage);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgAgentRelationship

		const string CreateOrgAgentRelationshipSql = @"INSERT INTO dbo.OrgAgentRelationship(O3_PK, O3_OH_SendingAgent, O3_SystemCreateTimeUtc, O3_SystemCreateUser, O3_SystemLastEditTimeUtc, O3_SystemLastEditUser) VALUES (@pk, @orgHeader, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgAgentRelationship(Guid orgHeader)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgAgentRelationshipSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@orgHeader", SqlDbType.UniqueIdentifier, orgHeader);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgCompanyData

		const string CreateOrgCompanyDataSql = @"INSERT INTO dbo.OrgCompanyData (OB_PK, OB_IsValid, OB_GC, OB_OH, OB_SystemCreateTimeUtc, OB_SystemCreateUser, OB_SystemLastEditTimeUtc, OB_SystemLastEditUser) VALUES (@pk, @isValid, @gc, @oh, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgCompanyData(Guid glbCompany, Guid orgHeader)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgCompanyDataSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@isValid", SqlDbType.Bit, 1);
				command.AddParameter("@gc", SqlDbType.UniqueIdentifier, glbCompany);
				command.AddParameter("@oh", SqlDbType.UniqueIdentifier, orgHeader);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgRelatedParty

		const string CreateOrgRelatedPartySql = @"
INSERT INTO dbo.OrgRelatedParty (PR_PK, PR_PartyType, PR_FreightDirection, PR_FreightTransportMode, PR_FreightContainerMode, PR_OH_RelatedParty, PR_OH_Parent, PR_GC, PR_SystemCreateTimeUtc, PR_SystemCreateUser, PR_SystemLastEditTimeUtc, PR_SystemLastEditUser)
VALUES (@pk, @partyType, @freightDirection, @freightTransportMode, @freightContainerMode, @relatedParty, @parent, @gcCompany, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgRelatedParty(string partyType, Guid relatedParty, Guid parent)
		{
			return CreateOrgRelatedParty(partyType, string.Empty, relatedParty, parent);
		}

		public Guid CreateOrgRelatedParty(string partyType, string freightDirection, Guid relatedParty, Guid parent, Guid gcCompany = default)
		{
			return CreateOrgRelatedParty(partyType, freightDirection, string.Empty, string.Empty, relatedParty, parent, gcCompany);
		}

		public Guid CreateOrgRelatedParty(string partyType, string freightDirection, string transportMode, string containerMode, Guid relatedParty, Guid parent, Guid gcCompany = default)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgRelatedPartySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@partyType", SqlDbType.VarChar, partyType);
				command.AddParameter("@freightDirection", SqlDbType.VarChar, freightDirection);
				command.AddParameter("@freightTransportMode", SqlDbType.VarChar, transportMode);
				command.AddParameter("@freightContainerMode", SqlDbType.VarChar, containerMode);
				command.AddParameter("@relatedParty", SqlDbType.UniqueIdentifier, relatedParty);
				command.AddParameter("@parent", SqlDbType.UniqueIdentifier, parent);
				command.AddParameter("@gcCompany", SqlDbType.UniqueIdentifier, gcCompany == default ? DBNull.Value : gcCompany);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobDocAddress

		const string CreateJobDocAddressSql = @"INSERT INTO dbo.JobDocAddress (E2_PK, E2_AddressType, E2_OA_Address, E2_ParentID, E2_ParentTableCode, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser) VALUES (@pk, @addressType, @oa, @parent, @parentTableCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobDocAddress(string addressType, Guid oa, Guid parent, string parentTableCode)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobDocAddressSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@addressType", SqlDbType.VarChar, addressType);
				command.AddParameter("@oa", SqlDbType.UniqueIdentifier, oa);
				command.AddParameter("@parent", SqlDbType.UniqueIdentifier, parent);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateProcessTaskTemplate

		const string CreateProcessTaskTemplateSql = @"INSERT INTO dbo.ProcessTaskTemplate(P0_PK, P0_IsSystem, P0_IsActive, P0_Name, P0_SystemCreateTimeUtc, P0_SystemCreateUser, P0_SystemLastEditTimeUtc, P0_SystemLastEditUser) VALUES(@pk, 0, 1, @name, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateProcessTaskTemplate(string name)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateProcessTaskTemplateSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@name", SqlDbType.VarChar, name);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateProcessTaskTemplateForSpecialTypeSql = @"INSERT INTO dbo.ProcessTaskTemplate(P0_PK, P0_IsSystem, P0_IsActive, P0_Name, P0_ProcessType, P0_FormState, P0_SubType1, P0_SubType2,P0_SystemCreateTimeUtc, P0_SystemCreateUser, P0_SystemLastEditTimeUtc, P0_SystemLastEditUser) VALUES('{0}', 0, 1, '{1}', '{2}', dbo.CLRCompressStringAsBytes('{3}'), '{4}', '{5}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateProcessTaskTemplate(string name, string processType, string formState, string subType1 = "", string subType2 = "")
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(string.Format(CreateProcessTaskTemplateForSpecialTypeSql, pk, name, processType, formState, subType1, subType2)))
			{
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateProcessTemplateTrigger

		const string CreateProcessTemplateTriggerSql = @"INSERT INTO dbo.ProcessTemplateTrigger (P9T_PK,P9T_P0_Template,P9T_SE_NKTriggerEvent,P9T_Description,P9T_Sequence,P9T_SystemCreateTimeUtc,P9T_SystemLastEditTimeUtc,P9T_SystemCreateUser,P9T_SystemLastEditUser) VALUES (@pk, @processTaskTemplatePk, @eventCode, 'Trigger', 1, GETDATE(), GETDATE(), '~BP', '~BP')";

		public Guid CreateProcessTemplateTrigger(Guid processTaskTemplatePk, string eventCode)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateProcessTemplateTriggerSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@processTaskTemplatePk", SqlDbType.UniqueIdentifier, processTaskTemplatePk);
				command.AddParameter("@eventCode", SqlDbType.VarChar, eventCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateProcessJobTriggerLink

		const string CreateProcessJobTriggerLinkSql = @"INSERT INTO dbo.ProcessJobTriggerLink (P9L_PK, P9L_P9T_TemplateTrigger, P9L_GC_Company,P9L_ParentTableCode, P9L_ParentId,P9L_TriggerFiredCountdown, P9L_SystemCreateTimeUtc, P9L_SystemCreateUser, P9L_SystemLastEditTimeUtc, P9L_SystemLastEditUser) VALUES (@pk, @processTemplateTriggerPk, @CompanyPK, @parentTableCode, @parentId, @triggerFiredCountdown, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateProcessJobTriggerLink(Guid processTaskTemplatePk, Guid companyPk, string parentTableCode, Guid parentId, int triggerFiredCountdown)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateProcessJobTriggerLinkSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@processTemplateTriggerPk", SqlDbType.UniqueIdentifier, processTaskTemplatePk);
				command.AddParameter("@companyPk", SqlDbType.UniqueIdentifier, companyPk);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@parentId", SqlDbType.UniqueIdentifier, parentId);
				command.AddParameter("@triggerFiredCountdown", SqlDbType.Int, triggerFiredCountdown);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGenAddOnColumn

		public Guid CreateGenAddOnColumn(string name, string data, string parentTableCode, Guid parentPK)
		{
			var pk = Guid.NewGuid();
			var sql = @"
INSERT dbo.GenAddOnColumn (XA_PK, XA_Name, XA_Data, XA_ParentTableCode, XA_ParentID, XA_SystemCreateTimeUtc, XA_SystemCreateUser, XA_SystemLastEditTimeUtc, XA_SystemLastEditUser)
VALUES (@pk, @name, @data, @parentTableCode, @parentPK, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@name", SqlDbType.VarChar, name);
				command.AddParameter("@data", SqlDbType.VarChar, data);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateScheduleTask

		const string CreateScheduleTaskSql = @"INSERT INTO [dbo].[StmScheduleTask] ([S5_PK], [S5_TaskPeriod], [S5_TaskPeriodCount], [S5_ScheduleType], [S5_IsActive], [S5_ParentTableCode], [S5_SystemCreateTimeUtc], [S5_SystemCreateUser], [S5_SystemLastEditTimeUtc], [S5_SystemLastEditUser]) VALUES (@pk, @taskPeriod, @taskPeriodCount, @scheduleType, @isActive, 'SH', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public void CreateScheduleTask(Guid scheduleTaskPk, string taskPeriod, int taskPeriodCount, string scheduleType, bool isActive)
		{
			using (var command = Db.Connection.Command(CreateScheduleTaskSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, scheduleTaskPk);
				command.AddParameter("@taskPeriod", SqlDbType.VarChar, taskPeriod);
				command.AddParameter("@taskPeriodCount", SqlDbType.Int, taskPeriodCount);
				command.AddParameter("@scheduleType", SqlDbType.VarChar, scheduleType);
				command.AddParameter("@isActive", SqlDbType.Bit, isActive);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region CreateStmModuleFilter

		const string CreateStmModuleFilterSql = @"
INSERT INTO [dbo].[StmModuleFilter] ([S9_PK], [S9_ModuleID], [S9_FilterName], S9_SystemCreateTimeUtc, S9_SystemCreateUser, S9_SystemLastEditTimeUtc, S9_SystemLastEditUser, [S9_GC])
VALUES (@pk, @moduleId, @filterName, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @companyPK)";

		const string CreateStmModuleFilterWithFilterDataSql = @"
INSERT INTO [dbo].[StmModuleFilter] ([S9_PK], [S9_ModuleID], [S9_FilterName], [S9_FilterData], S9_SystemCreateTimeUtc, S9_SystemCreateUser, S9_SystemLastEditTimeUtc, S9_SystemLastEditUser, [S9_GC])
VALUES (@pk, @moduleId, @filterName, @filterData, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @companyPK)";

		const string CreateStmModuleFilterWithCompressedFilterDataSql = @"
INSERT INTO [dbo].[StmModuleFilter] ([S9_PK], [S9_ModuleID], [S9_FilterName], [S9_FilterData], S9_SystemCreateTimeUtc, S9_SystemCreateUser, S9_SystemLastEditTimeUtc, S9_SystemLastEditUser, [S9_GC])
VALUES (@pk, @moduleId, @filterName, dbo.CLRCompressAsBytes(@filterData), GetUtcDate(), '~BP', GetUtcDate(), '~BP', @companyPK)";

		public Guid CreateModuleFilter(string moduleId, string filterName, byte[] filterData = null, Guid? companyPK = null, bool compressFilterData = false)
		{
			var pk = Guid.NewGuid();
			var query = CreateStmModuleFilterSql;
			if (filterData is not null)
			{
				query = compressFilterData ? CreateStmModuleFilterWithCompressedFilterDataSql : CreateStmModuleFilterWithFilterDataSql;
			}

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@moduleId", SqlDbType.VarChar, StmModuleFilterSchema.S9_ModuleID.MaxLength, moduleId);
				command.AddParameter("@filterName", SqlDbType.NVarChar, StmModuleFilterSchema.S9_FilterName.MaxLength, filterName);
				if (filterData != null)
				{
					command.AddParameter("@filterData", SqlDbType.VarBinary, StmModuleFilterSchema.S9_FilterData.MaxLength, filterData);
				}
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, (object)companyPK ?? DBNull.Value);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateStmModuleFilterUserData
		const string CreateStmModuleFilterUserDataSql = @"
INSERT INTO [dbo].[StmModuleFilterUserData] ([S0_PK], [S0_RelatedEntityTableCode], [S0_RelatedEntityID], [S0_S9], [S0_SystemCreateTimeUtc], [S0_SystemCreateUser], [S0_SystemLastEditTimeUtc], [S0_SystemLastEditUser])
VALUES (@pk, @relatedEntityTableCode, @relatedEntityId, @moduleFilterId, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";
		const string CreateStmModuleFilterUserDataWithDataValuesSql = @"
INSERT INTO [dbo].[StmModuleFilterUserData] ([S0_PK], [S0_RelatedEntityTableCode], [S0_RelatedEntityID], [S0_S9], [S0_FilterDataValues], [S0_SystemCreateTimeUtc], [S0_SystemCreateUser], [S0_SystemLastEditTimeUtc], [S0_SystemLastEditUser])
VALUES (@pk, @relatedEntityTableCode, @relatedEntityId, @moduleFilterId, @filterDataValues, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateModuleFilterUserData(string relatedEntityTableCode, Guid relatedEntityId, Guid moduleFilterId, byte[] filterDataValues = null)
		{
			var pk = Guid.NewGuid();
			var query = (filterDataValues == null) ? CreateStmModuleFilterUserDataSql : CreateStmModuleFilterUserDataWithDataValuesSql;

			using (var command = Db.Connection.Command(query))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@relatedEntityTableCode", SqlDbType.VarChar, StmModuleFilterUserDataSchema.S0_RelatedEntityTableCode.MaxLength, relatedEntityTableCode);
				command.AddParameter("@relatedEntityId", SqlDbType.UniqueIdentifier, relatedEntityId);
				command.AddParameter("@moduleFilterId", SqlDbType.UniqueIdentifier, moduleFilterId);
				if (filterDataValues != null)
				{
					command.AddParameter("@filterDataValues", SqlDbType.VarBinary, filterDataValues);
				}
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateStmNote

		const string CreateStmNoteSql = @"
INSERT INTO [dbo].[StmNote] ([ST_PK], [ST_ParentID], [ST_Table], [ST_NoteText], [ST_NoteType], [ST_Description], [ST_SystemCreateTimeUtc], [ST_SystemCreateUser], [ST_SystemLastEditTimeUtc], [ST_SystemLastEditUser])
VALUES (@pk, @parentId, @parentTable, @noteText, @noteType, @noteDescription, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateStmNote(string parentTable, Guid parentId, string noteText, string noteType, string noteDescription)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateStmNoteSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentTable", SqlDbType.VarChar, StmNoteSchema.ST_Table.MaxLength, parentTable);
				command.AddParameter("@parentId", SqlDbType.UniqueIdentifier, parentId);
				command.AddParameter("@noteText", SqlDbType.NVarChar, noteText);
				command.AddParameter("@noteType", SqlDbType.Char, StmNoteSchema.ST_NoteType.MaxLength, noteType);
				command.AddParameter("@noteDescription", SqlDbType.NVarChar, StmNoteSchema.ST_Description.MaxLength, noteDescription);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateStmNoteTemplate

		const string CreateStmNoteTemplateSql = @"INSERT INTO [dbo].[StmNoteTemplate] ([S8_PK], [S8_ContextID], [S8_GS_NKStaff], [S8_Description], [S8_TemplateText], [S8_GC], [S8_SystemCreateUser], [S8_SystemCreateTimeUtc], [S8_SystemLastEditUser], [S8_SystemLastEditTimeUtc])
		VALUES (@pk, @contextId, 'E', @description, @templateText, @companyPk, 'E', GETDATE(), 'E', GETDATE())";

		public Guid CreateStmNoteTemplate(Guid contextId, string description, string templateText, Guid companyPk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateStmNoteTemplateSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@contextId", SqlDbType.UniqueIdentifier, contextId);
				command.AddParameter("@description", SqlDbType.NVarChar, description);
				command.AddParameter("@templateText", SqlDbType.NVarChar, templateText);
				command.AddParameter("@companyPk", SqlDbType.UniqueIdentifier, companyPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefLocalLanguage

		const string CreateRefLocalLanguageSql = @"
INSERT INTO [dbo].[RefLocalLanguage] ([RA_PK], [RA_Code], [RA_Description], [RA_RN_NKCountryCode], [RA_SystemCreateTimeUtc], [RA_SystemCreateUser], [RA_SystemLastEditTimeUtc], [RA_SystemLastEditUser])
VALUES (@pk, @code, @description, @countryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefLocalLanguage(string code, string description, string countryCode)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefLocalLanguageSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, RefLocalLanguageSchema.RA_Code.MaxLength, code);
				command.AddParameter("@description", SqlDbType.VarChar, RefLocalLanguageSchema.RA_Description.MaxLength, description);
				command.AddParameter("@countryCode", SqlDbType.VarChar, RefLocalLanguageSchema.RA_RN_NKCountryCode.MaxLength, countryCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateStmPrintQueue

		const string CreateStmPrintQueueSql = @"
INSERT INTO [dbo].[StmPrintQueue] ([SQ_PK], [SQ_QueueName], [SQ_PrintQueueStateChanged], [SQ_DisplayName], [SQ_SPS_Server], [SQ_SystemCreateTimeUtc], [SQ_SystemCreateUser], [SQ_SystemLastEditTimeUtc], [SQ_SystemLastEditUser])
VALUES (@pk, @queueName, @stateChanged, @displayName, @serverPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreatePrintQueue(string queueName, string displayName, Guid serverPk)
		{
			var pk = Guid.NewGuid();
			var printQueueStateChanged = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateStmPrintQueueSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@stateChanged", SqlDbType.UniqueIdentifier, printQueueStateChanged);
				command.AddParameter("@queueName", SqlDbType.NVarChar, StmPrintQueueSchema.SQ_QueueName.MaxLength, queueName);
				command.AddParameter("@displayName", SqlDbType.NVarChar, StmPrintQueueSchema.SQ_DisplayName.MaxLength, displayName);
				command.AddParameter("@serverPk", SqlDbType.UniqueIdentifier, serverPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateStmPrintJob

		const string CreateStmPrintJobSql = @"
INSERT INTO [dbo].[StmPrintJob] ([SP_PK], [SP_JobType], [SP_ParentGuid], [SP_ParentTableName], [SP_EmailAttachments], [SP_DocumentName], [SP_SystemCreateTimeUtc], [SP_SystemCreateUser], [SP_SystemLastEditTimeUtc], [SP_SystemLastEditUser])
VALUES (@pk, @jobType, @parentGuid, @parentTableName, @emailAttachments, @documentName, @systemCreateTimeUtc, '~BP', GetUtcDate(), '~BP')";

		public Guid CreatePrintJob(string jobType, Guid parentGuid, string parentTableName, string emailAttachments, string documentName, DateTime systemCreateTimeUtc)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateStmPrintJobSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobType", SqlDbType.Char, jobType);
				command.AddParameter("@parentGuid", SqlDbType.UniqueIdentifier, parentGuid);
				command.AddParameter("@parentTableName", SqlDbType.VarChar, parentTableName);
				command.AddParameter("@emailAttachments", SqlDbType.NVarChar, emailAttachments);
				command.AddParameter("@documentName", SqlDbType.NVarChar, documentName);
				command.AddParameter("@systemCreateTimeUtc", SqlDbType.DateTime, systemCreateTimeUtc);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateStmPrintServer

		const string CreateStmPrintServerSql = @"
INSERT INTO [dbo].[StmPrintServer] ([SPS_PK], [SPS_ServerName], [SPS_SystemCreateTimeUtc], [SPS_SystemCreateUser], [SPS_SystemLastEditTimeUtc], [SPS_SystemLastEditUser])
VALUES (@pk, @serverName, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreatePrintServer(string serverName)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateStmPrintServerSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@serverName", SqlDbType.VarChar, StmPrintServerSchema.SPS_ServerName.MaxLength, serverName);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGenShapeGeography

		const string CreateGenShapeGeographySql = @"
INSERT INTO [dbo].[GenShapeGeography] ([SHG_PK], [SHG_Name], [SHG_Description], [SHG_Type], [SHG_SystemCreateTimeUtc], [SHG_SystemCreateUser], [SHG_SystemLastEditTimeUtc], [SHG_SystemLastEditUser])
VALUES (@pk, @name, @description, @type, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGenShapeGeography(string name, string description, string type)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGenShapeGeographySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@name", SqlDbType.VarChar, GenShapeGeographySchema.SHG_Name.MaxLength, name);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@type", SqlDbType.VarChar, GenShapeGeographySchema.SHG_Type.MaxLength, type);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGlbCapability

		const string CreateGlbCapabilitySql = @"
INSERT INTO [dbo].[GlbCapability] ([G4_PK], [G4_Code], [G4_Description], [G4_CapacityScope], [G4_SystemCreateTimeUtc], [G4_SystemCreateUser], [G4_SystemLastEditTimeUtc], [G4_SystemLastEditUser])
VALUES (@pk, @code, @description, @capacityScope, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGlbCapability(string code, string description, string capacityScope)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbCapabilitySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, GlbCapabilitySchema.G4_Code.MaxLength, code);
				command.AddParameter("@description", SqlDbType.VarChar, GlbCapabilitySchema.G4_Description.MaxLength, description);
				command.AddParameter("@capacityScope", SqlDbType.VarChar, GlbCapabilitySchema.G4_CapacityScope.MaxLength, capacityScope);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGlbCompanyCampaign

		const string CreateGlbCompanyCampaignSql = @"INSERT INTO dbo.GlbCompanyCampaign(G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_BroadcastVoteSurveyExam, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser, G0_BatchCountDefault, G0_LastSentBatchNumber, G0_QuestionsPerWebPage) VALUES (@pk, @companyPk, @name, @campaignID, @type, GetUtcDate(), '~BP', GetUtcDate(), '~BP', @batchCountDefault, @lastSentBatchNumber, @questionsPerWebPage)";

		const string CreateGlbCompanyCampaignWithCreateTimeSql = @"INSERT INTO dbo.GlbCompanyCampaign(G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_BroadcastVoteSurveyExam, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser) VALUES (@pk, @companyPk, @name, @campaignID, @type, @createTime, '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGlbCompanyCampaign(string name, string type, Guid companyPk, string campaignID, int batchCountDefault = 0, int lastSentBatchNumber = 0, int questionsPerWebPage = 0)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbCompanyCampaignSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, GlbCompanyCampaignSchema.PK);
				command.AddParameterBasedOnDbColumn("@companyPk", companyPk, GlbCompanyCampaignSchema.G0_GC);
				command.AddParameterBasedOnDbColumn("@name", name, GlbCompanyCampaignSchema.G0_CampaignName);
				command.AddParameterBasedOnDbColumn("@type", type, GlbCompanyCampaignSchema.G0_BroadcastVoteSurveyExam);
				command.AddParameterBasedOnDbColumn("@campaignID", campaignID, GlbCompanyCampaignSchema.G0_CampaignID);
				command.AddParameterBasedOnDbColumn("@batchCountDefault", batchCountDefault, GlbCompanyCampaignSchema.G0_BatchCountDefault);
				command.AddParameterBasedOnDbColumn("@lastSentBatchNumber", lastSentBatchNumber, GlbCompanyCampaignSchema.G0_LastSentBatchNumber);
				command.AddParameterBasedOnDbColumn("@questionsPerWebPage", questionsPerWebPage, GlbCompanyCampaignSchema.G0_QuestionsPerWebPage);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid CreateGlbCompanyCampaign(string name, string type, Guid companyPk, string campaignID, DateTime? createTime)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbCompanyCampaignWithCreateTimeSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, GlbCompanyCampaignSchema.PK);
				command.AddParameterBasedOnDbColumn("@companyPk", companyPk, GlbCompanyCampaignSchema.G0_GC);
				command.AddParameterBasedOnDbColumn("@name", name, GlbCompanyCampaignSchema.G0_CampaignName);
				command.AddParameterBasedOnDbColumn("@type", type, GlbCompanyCampaignSchema.G0_BroadcastVoteSurveyExam);
				command.AddParameterBasedOnDbColumn("@campaignID", campaignID, GlbCompanyCampaignSchema.G0_CampaignID);
				command.AddParameterBasedOnDbColumn("@createTime", createTime == null ? DBNull.Value : createTime, GlbCompanyCampaignSchema.G0_SystemCreateTimeUtc);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGlbCompanyCampaignBudgetItem

		const string CreateGlbCompanyCampaignBudgetItemSql = @"INSERT INTO dbo.GlbCompanyCampaignBudgetItem(G9_PK, G9_G0, G9_ExchangeRate, G9_FlatAmount, G9_PerUnitAmount, G9_SystemCreateTimeUtc, G9_SystemCreateUser, G9_SystemLastEditTimeUtc, G9_SystemLastEditUser) VALUES (@pk, @campaignPk, @exchangeRate, @flatAmount, @perUnitAmount, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGlbCompanyCampaignBudgetItem(Guid campaignPk, decimal exchangeRate = 0, decimal flatAmount = 0, decimal perUnitAmount = 0)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbCompanyCampaignBudgetItemSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, GlbCompanyCampaignBudgetItemSchema.PK);
				command.AddParameterBasedOnDbColumn("@campaignPk", campaignPk, GlbCompanyCampaignBudgetItemSchema.G9_G0);
				command.AddParameterBasedOnDbColumn("@exchangeRate", exchangeRate, GlbCompanyCampaignBudgetItemSchema.G9_ExchangeRate);
				command.AddParameterBasedOnDbColumn("@flatAmount", flatAmount, GlbCompanyCampaignBudgetItemSchema.G9_FlatAmount);
				command.AddParameterBasedOnDbColumn("@perUnitAmount", perUnitAmount, GlbCompanyCampaignBudgetItemSchema.G9_PerUnitAmount);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGlbCompanyCampaignGroup

		const string CreateGlbCompanyCampaignGroupSql = @"INSERT INTO dbo.GlbCompanyCampaignGroup(GCG_PK, GCG_GroupColor, GCG_SystemCreateTimeUtc, GCG_SystemCreateUser, GCG_SystemLastEditTimeUtc, GCG_SystemLastEditUser) VALUES (@pk, @groupColor, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGlbCompanyCampaignGroup(int groupColor = 0)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbCompanyCampaignGroupSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, GlbCompanyCampaignGroupSchema.PK);
				command.AddParameterBasedOnDbColumn("@groupColor", groupColor, GlbCompanyCampaignGroupSchema.GCG_GroupColor);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGlbCompanyCampaignDripMarketing

		const string CreateGlbCompanyCampaignDripMarketingSql = @"INSERT INTO dbo.GlbCompanyCampaignDripMarketing(GCD_PK, GCD_G0_ParentTouch, GCD_G0_NextTouch, GCD_GCG_Group, GCD_SystemCreateTimeUtc, GCD_SystemCreateUser, GCD_SystemLastEditTimeUtc, GCD_SystemLastEditUser) VALUES (@pk, @parentTouch, @nextTouch, @groupPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGlbCompanyCampaignDripMarketing(Guid parentTouch, Guid nextTouch = default, Guid groupPk = default)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbCompanyCampaignDripMarketingSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentTouch", SqlDbType.UniqueIdentifier, parentTouch);
				command.AddParameter("@nextTouch", SqlDbType.UniqueIdentifier, nextTouch == default ? DBNull.Value : nextTouch);
				command.AddParameter("@groupPk", SqlDbType.UniqueIdentifier, groupPk == default ? DBNull.Value : groupPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGlbCompanyCampaignItems

		const string CreateGlbCompanyCampaignItemSql = @"
INSERT INTO [dbo].[GlbCompanyCampaignItem] ([G8_PK], [G8_G0],[G8_DeliveryMethod],[G8_Stage],[G8_GS_NKFollowedUpBy],[G8_IsValid],[G8_RecipientTableCode],[G8_RecipientID],
[G8_TrackingStatus],[G8_GS_NKSender],[G8_EmailSenderName],[G8_SenderEmailAddress],[G8_IsSuspended],[G8_IsBlocked],[G8_IsCheckTransitionRequired],[G8_BatchNumber],[G8_SystemCreateTimeUtc],[G8_SystemCreateUser],[G8_SystemLastEditTimeUtc],[G8_SystemLastEditUser])
VALUES (@pk, @campaignPk, @deliveryMethod, @stage, @followedUpBy, @isValid, @recipientTableCode, @recipientId, @trackingStatus, @sender, @emailSenderName, @senderEmailAddress, @isSuspended, @isBlocked, @isCheckTransitionRequired, @batchNumber, GetUtcDate() , '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGlbCompanyCampaignItem(Guid campaignPk, Guid recipientId, string recipientTableCode = "", string deliveryMethod = "", string stage = "", string followedUpBy = "", bool isValid = true, string trackingStatus = "", string sender = "", string emailSenderName = "", string senderEmailAddress = "", bool isSuspended = false, bool isBlocked = false, bool isCheckTransitionRequired = false, int batchNumber = 0)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbCompanyCampaignItemSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, GlbCompanyCampaignItemSchema.PK);
				command.AddParameterBasedOnDbColumn("@campaignPk", campaignPk, GlbCompanyCampaignItemSchema.G8_G0);
				command.AddParameterBasedOnDbColumn("@deliveryMethod", deliveryMethod, GlbCompanyCampaignItemSchema.G8_DeliveryMethod);
				command.AddParameterBasedOnDbColumn("@stage", stage, GlbCompanyCampaignItemSchema.G8_Stage);
				command.AddParameterBasedOnDbColumn("@followedUpBy", followedUpBy, GlbCompanyCampaignItemSchema.G8_GS_NKFollowedUpBy);
				command.AddParameterBasedOnDbColumn("@isValid", isValid, GlbCompanyCampaignItemSchema.G8_IsValid);
				command.AddParameterBasedOnDbColumn("@recipientTableCode", recipientTableCode, GlbCompanyCampaignItemSchema.G8_RecipientTableCode);
				command.AddParameterBasedOnDbColumn("@recipientId", recipientId, GlbCompanyCampaignItemSchema.G8_RecipientID);
				command.AddParameterBasedOnDbColumn("@trackingStatus", trackingStatus, GlbCompanyCampaignItemSchema.G8_TrackingStatus);
				command.AddParameterBasedOnDbColumn("@sender", sender, GlbCompanyCampaignItemSchema.G8_GS_NKSender);
				command.AddParameterBasedOnDbColumn("@emailSenderName", emailSenderName, GlbCompanyCampaignItemSchema.G8_EmailSenderName);
				command.AddParameterBasedOnDbColumn("@senderEmailAddress", senderEmailAddress, GlbCompanyCampaignItemSchema.G8_SenderEmailAddress);
				command.AddParameterBasedOnDbColumn("@isSuspended", isSuspended, GlbCompanyCampaignItemSchema.G8_IsSuspended);
				command.AddParameterBasedOnDbColumn("@isBlocked", isBlocked, GlbCompanyCampaignItemSchema.G8_IsBlocked);
				command.AddParameterBasedOnDbColumn("@isCheckTransitionRequired", isCheckTransitionRequired, GlbCompanyCampaignItemSchema.G8_IsCheckTransitionRequired);
				command.AddParameterBasedOnDbColumn("@batchNumber", batchNumber, GlbCompanyCampaignItemSchema.G8_BatchNumber);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGlbCompanyCampaignSendSettings

		const string CreateGlbCompanyCampaignSendSettingsSql = @"INSERT INTO dbo.GlbCompanyCampaignSendSettings(GSC_PK, GSC_ContactLimitPerOrganizationInHorizontal, GSC_ScheduleType, GSC_SystemCreateTimeUtc, GSC_SystemCreateUser, GSC_SystemLastEditTimeUtc, GSC_SystemLastEditUser) VALUES (@pk, @contactLimitPerOrganizationInHorizontal, 'BAT', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateGlbCompanyCampaignSendSettings(short contactLimitPerOrganizationInHorizontal = 0)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbCompanyCampaignSendSettingsSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, GlbCompanyCampaignSendSettingsSchema.PK);
				command.AddParameterBasedOnDbColumn("@contactLimitPerOrganizationInHorizontal", contactLimitPerOrganizationInHorizontal, GlbCompanyCampaignSendSettingsSchema.GSC_ContactLimitPerOrganizationInHorizontal);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateGlbReleaseNote

		const string CreateGlbReleaseNoteSql = @"INSERT INTO [dbo].[GlbReleaseNote] ([GF_PK], [GF_IsValid], [GF_Category], [GF_RN_NKCountryForReleaseNote], [GF_Summary], [GF_URL], [GF_Section], [GF_ReleaseNoteDate], [GF_InstallationDate]) VALUES (@pk, 1, @category, @country, @summary, @url, @section, GETDATE(), GETDATE())";

		public Guid CreateGlbReleaseNote(string category, string country, string summary, string section, string url)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateGlbReleaseNoteSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, GlbReleaseNoteSchema.PK);
				command.AddParameterBasedOnDbColumn("@category", category, GlbReleaseNoteSchema.GF_Category);
				command.AddParameterBasedOnDbColumn("@country", country, GlbReleaseNoteSchema.GF_RN_NKCountryForReleaseNote);
				command.AddParameterBasedOnDbColumn("@summary", summary, GlbReleaseNoteSchema.GF_Summary);
				command.AddParameterBasedOnDbColumn("@section", section, GlbReleaseNoteSchema.GF_Section);
				command.AddParameterBasedOnDbColumn("@url", url, GlbReleaseNoteSchema.GF_URL);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobRequiredDocument

		const string CreateJobRequiredDocumentSql = @"
INSERT INTO [dbo].[JobRequiredDocument] ([EQ_PK], [EQ_DocType], [EQ_DocUsage], [EQ_DocPeriod], [EQ_DocDescription], [EQ_ParentID], [EQ_ParentTableCode], [EQ_DocCategory], [EQ_SystemCreateTimeUtc], [EQ_SystemCreateUser], [EQ_SystemLastEditTimeUtc], [EQ_SystemLastEditUser])
VALUES (@pk, @docType, @docUsage, @docPeriod, @docDescription, @parentPk, @parentCode, @docCategory, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobRequiredDocument(string docType, string docUsage, string docPeriod, string docDescription, string parentCode, string docCategory, Guid parentPk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobRequiredDocumentSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentPk", SqlDbType.UniqueIdentifier, parentPk);
				command.AddParameter("@docType", SqlDbType.VarChar, JobRequiredDocumentSchema.EQ_DocType.MaxLength, docType);
				command.AddParameter("@docUsage", SqlDbType.VarChar, JobRequiredDocumentSchema.EQ_DocUsage.MaxLength, docUsage);
				command.AddParameter("@docPeriod", SqlDbType.VarChar, JobRequiredDocumentSchema.EQ_DocPeriod.MaxLength, docPeriod);
				command.AddParameter("@docDescription", SqlDbType.VarChar, JobRequiredDocumentSchema.EQ_DocDescription.MaxLength, docDescription);
				command.AddParameter("@parentCode", SqlDbType.VarChar, JobRequiredDocumentSchema.EQ_ParentTableCode.MaxLength, parentCode);
				command.AddParameter("@docCategory", SqlDbType.VarChar, JobRequiredDocumentSchema.EQ_DocCategory.MaxLength, docCategory);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region JobRequiredDocumentAddInfo

		const string CreateJobRequiredDocumentAddInfoSql = @"
INSERT INTO [dbo].[JobRequiredDocumentAddInfo] ([EX_PK], [EX_ApplicationCode], [EX_AddInfo], [EX_EQ_RequiredDocument], [EX_GC_Company], [EX_Status], [EX_ReferenceNumber], [EX_SystemCreateTimeUtc], [EX_SystemCreateUser], [EX_SystemLastEditTimeUtc], [EX_SystemLastEditUser])
VALUES (@pk, @applicationCode, @addInfo, @jobRequiredDocumentPk, @company, @status, @referenceNumber, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobRequiredDocumentAddInfo(string applicationCode, string addInfo, Guid jobRequiredDocumentPk, Guid company, string status = null, string referenceNumber = null)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobRequiredDocumentAddInfoSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobRequiredDocumentPk", SqlDbType.UniqueIdentifier, jobRequiredDocumentPk);
				command.AddParameter("@applicationCode", SqlDbType.VarChar, JobRequiredDocumentAddInfoSchema.EX_ApplicationCode.MaxLength, applicationCode);
				command.AddParameter("@addInfo", SqlDbType.VarChar, JobRequiredDocumentAddInfoSchema.EX_AddInfo.MaxLength, addInfo);
				command.AddParameter("@company", SqlDbType.UniqueIdentifier, company);
				command.AddParameter("@status", SqlDbType.VarChar, JobRequiredDocumentAddInfoSchema.EX_Status.MaxLength, status ?? "");
				command.AddParameter("@referenceNumber", SqlDbType.VarChar, JobRequiredDocumentAddInfoSchema.EX_ReferenceNumber.MaxLength, referenceNumber ?? "");
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region JobRequiredDocAttrib

		const string CreateJobRequiredDocAttribSql = @"
INSERT INTO [dbo].[JobRequiredDocAttrib] ([D0_PK], [D0_AttribName], [D0_AttribValue], [D0_EQ], [D0_SystemCreateTimeUtc], [D0_SystemCreateUser], [D0_SystemLastEditTimeUtc], [D0_SystemLastEditUser])
VALUES (@pk, @attribName, @attribValue, @jobRequiredDocumentPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobRequiredDocAttrib(string attribName, string attribValue, Guid jobRequiredDocumentPk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateJobRequiredDocAttribSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@jobRequiredDocumentPk", SqlDbType.UniqueIdentifier, jobRequiredDocumentPk);
				command.AddParameter("@attribName", SqlDbType.VarChar, JobRequiredDocAttribSchema.D0_AttribName.MaxLength, attribName);
				command.AddParameter("@attribValue", SqlDbType.VarChar, JobRequiredDocAttribSchema.D0_AttribValue.MaxLength, attribValue);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgContactAttribute

		const string CreateOrgContactAttributeSql = @"
INSERT INTO [dbo].[OrgContactAttribute] ([PC_PK], [PC_Type], [PC_OC], [PC_SystemCreateTimeUtc], [PC_SystemCreateUser], [PC_SystemLastEditTimeUtc], [PC_SystemLastEditUser])
VALUES (@pk, @type, @contactPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgContactAttribute(string type, Guid contactPk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgContactAttributeSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@contactPk", SqlDbType.UniqueIdentifier, contactPk);
				command.AddParameter("@type", SqlDbType.VarChar, OrgContactAttributeSchema.PC_Type.MaxLength, type);
				command.ExecuteNonQuery
();
			}

			return pk;
		}
		#endregion

		#region CreateOrgDocument

		const string CreateOrgDocumentSql = @"
INSERT INTO [dbo].[OrgDocument] ([OD_PK], [OD_DocumentGroup], [OD_FilterShipmentMode], [OD_FilterDirection], [OD_OC], [OD_AttachmentType], [OD_DeliverBy], [OD_SystemCreateTimeUtc], [OD_SystemCreateUser], [OD_SystemLastEditTimeUtc], [OD_SystemLastEditUser])
VALUES (@pk, @documentGroup, @shipmentMode, @filterDirection, @contactPk, @attachmentType, @deliverBy, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgDocument(string documentGroup, Guid contactPk)
		{
			var pk = Guid.NewGuid();
			const string shipmentMode = "ALL";
			const string filterDirection = "ALL";
			const string attachmentType = "PDF";
			const string deliverBy = "EML";

			using (var command = Db.Connection.Command(CreateOrgDocumentSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@documentGroup", SqlDbType.VarChar, OrgDocumentSchema.OD_DocumentGroup.MaxLength, documentGroup);
				command.AddParameter("@shipmentMode", SqlDbType.VarChar, OrgDocumentSchema.OD_FilterShipmentMode.MaxLength, shipmentMode);
				command.AddParameter("@filterDirection", SqlDbType.VarChar, OrgDocumentSchema.OD_FilterDirection.MaxLength, filterDirection);
				command.AddParameter("@contactPk", SqlDbType.UniqueIdentifier, contactPk);
				command.AddParameter("@attachmentType", SqlDbType.VarChar, OrgDocumentSchema.OD_AttachmentType.MaxLength, attachmentType);
				command.AddParameter("@deliverBy", SqlDbType.VarChar, OrgDocumentSchema.OD_DeliverBy.MaxLength, deliverBy);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateCrmOpportunity

		public Guid CreateCrmOpportunity(string oppID, string oppName, Guid orgPk, Guid companyPk, string frequencyUnit, string salesPerson = "", bool isRestricted = false, decimal revenue = 0, decimal profit = 0)
		{
			var oppPK = Guid.NewGuid();

			var sql = @$"
INSERT INTO dbo.CrmOpportunity(COP_PK, COP_OpportunityID, COP_OpportunityName, COP_OH_Organization, COP_GC_Company, COP_GS_NKSalesPerson, COP_IsRestricted, COP_RX_NKOverallCurrency, COP_OverallFrequencyUnit, COP_EstimatedRevenue, COP_EstimatedProfit, COP_SystemCreateTimeUtc, COP_SystemCreateUser, COP_SystemLastEditTimeUtc, COP_SystemLastEditUser)
VALUES (@oppPK, @oppID, @oppName, @orgPk, @companyPk, @salesPerson, @isRestricted, @currency, @frequencyUnit, @revenue, @profit, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@oppPK", oppPK, CrmOpportunitySchema.PK);
				command.AddParameterBasedOnDbColumn("@oppID", oppID, CrmOpportunitySchema.COP_OpportunityID);
				command.AddParameterBasedOnDbColumn("@oppName", oppName, CrmOpportunitySchema.COP_OpportunityName);
				command.AddParameterBasedOnDbColumn("@orgPk", orgPk, CrmOpportunitySchema.COP_OH_Organization);
				command.AddParameterBasedOnDbColumn("@companyPk", companyPk, CrmOpportunitySchema.COP_GC_Company);
				command.AddParameterBasedOnDbColumn("@salesPerson", salesPerson, CrmOpportunitySchema.COP_GS_NKSalesPerson);
				command.AddParameterBasedOnDbColumn("@isRestricted", isRestricted, CrmOpportunitySchema.COP_IsRestricted);
				command.AddParameterBasedOnDbColumn("@currency", "AUD", CrmOpportunitySchema.COP_RX_NKOverallCurrency);
				command.AddParameterBasedOnDbColumn("@frequencyUnit", frequencyUnit, CrmOpportunitySchema.COP_OverallFrequencyUnit);
				command.AddParameterBasedOnDbColumn("@revenue", revenue, CrmOpportunitySchema.COP_EstimatedRevenue);
				command.AddParameterBasedOnDbColumn("@profit", profit, CrmOpportunitySchema.COP_EstimatedProfit);
				command.ExecuteNonQuery();
			}
			return oppPK;
		}

		#endregion

		#region CreateCrmOpportunityScope

		public Guid CreateCrmOpportunityScope(Guid oppId, byte scopeId, string productCode, string origin, string destination, string frequencyUnit, decimal revenue = 0, decimal profit = 0)
		{
			var scopeOppPK = Guid.NewGuid();

			var sql = @$"
INSERT INTO dbo.CrmOpportunityScope(COS_PK, COS_COP_Opportunity, COS_ScopeID, COS_ProductCode, COS_Origin, COS_Destination, COS_Enabled, COS_RX_NKScopeCurrency, COS_ScopeFrequencyUnit, COS_EstimatedRevenue, COS_EstimatedProfit, COS_SystemCreateTimeUtc, COS_SystemCreateUser, COS_SystemLastEditTimeUtc, COS_SystemLastEditUser)
VALUES (@scopeOppPK, @oppId, @scopeId, @productCode, @origin, @destination, @Enabled, @currency, @frequencyUnit, @revenue, @profit, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameterBasedOnDbColumn("@scopeOppPK", scopeOppPK, CrmOpportunityScopeSchema.PK);
				command.AddParameterBasedOnDbColumn("@oppId", oppId, CrmOpportunityScopeSchema.COS_COP_Opportunity);
				command.AddParameterBasedOnDbColumn("@scopeId", scopeId, CrmOpportunityScopeSchema.COS_ScopeID);
				command.AddParameterBasedOnDbColumn("@productCode", productCode, CrmOpportunityScopeSchema.COS_ProductCode);
				command.AddParameterBasedOnDbColumn("@origin", origin, CrmOpportunityScopeSchema.COS_Origin);
				command.AddParameterBasedOnDbColumn("@destination", destination, CrmOpportunityScopeSchema.COS_Destination);
				command.AddParameterBasedOnDbColumn("@Enabled", true, CrmOpportunityScopeSchema.COS_Enabled);
				command.AddParameterBasedOnDbColumn("@currency", "AUD", CrmOpportunityScopeSchema.COS_RX_NKScopeCurrency);
				command.AddParameterBasedOnDbColumn("@frequencyUnit", frequencyUnit, CrmOpportunityScopeSchema.COS_ScopeFrequencyUnit);
				command.AddParameterBasedOnDbColumn("@revenue", revenue, CrmOpportunityScopeSchema.COS_EstimatedRevenue);
				command.AddParameterBasedOnDbColumn("@profit", profit, CrmOpportunityScopeSchema.COS_EstimatedProfit);
				command.ExecuteNonQuery();
			}
			return scopeOppPK;
		}

		#endregion

		const string CreateOrgMiscServWithClientIntelSql = @"
INSERT INTO [dbo].[OrgMiscServ] ([OM_PK], [OM_OH], [OM_CICapitalEmployed], [OM_CIEstimatedStaffThisCountry], [OM_CIEstimatedStaffThisLocation], [OM_CITurnover], [OM_CIProfit], [OM_SystemCreateTimeUtc], [OM_SystemCreateUser], [OM_SystemLastEditTimeUtc], [OM_SystemLastEditUser])
VALUES (@pk, @orgHeaderPk, @capitalEmployed, @cIEstimatedStaffThisCountry, @cIEstimatedStaffThisLocation, @cITurnover, @cIProfit, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgMiscServWithClientIntel(Guid orgHeaderPk, decimal capitalEmployed, short estimatedStaffThisCountry, short estimatedStaffThisLocation, decimal turnOver, decimal profit)
		{
			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command(CreateOrgMiscServWithClientIntelSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, OrgMiscServSchema.PK);
				command.AddParameterBasedOnDbColumn("@orgHeaderPk", orgHeaderPk, OrgMiscServSchema.OM_OH);
				command.AddParameterBasedOnDbColumn("@capitalEmployed", capitalEmployed, OrgMiscServSchema.OM_CICapitalEmployed);
				command.AddParameterBasedOnDbColumn("@cIEstimatedStaffThisCountry", estimatedStaffThisCountry, OrgMiscServSchema.OM_CIEstimatedStaffThisCountry);
				command.AddParameterBasedOnDbColumn("@cIEstimatedStaffThisLocation", estimatedStaffThisLocation, OrgMiscServSchema.OM_CIEstimatedStaffThisLocation);
				command.AddParameterBasedOnDbColumn("@cITurnover", turnOver, OrgMiscServSchema.OM_CITurnover);
				command.AddParameterBasedOnDbColumn("@cIProfit", profit, OrgMiscServSchema.OM_CIProfit);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		const string CreateOrgMiscServWithCMSql = @"
INSERT INTO [dbo].[OrgMiscServ] ([OM_PK], [OM_OH], [OM_CMAcheivableClientRevenue], [OM_CMConsultingRevenue], [OM_CMEstimatedProfit], [OM_CMNoOfEmployees], [OM_CMPaidUpCapital], [OM_CMPercentage], [OM_CMTotalClientRevenue], [OM_CMWarehouseRevenue], [OM_SystemCreateTimeUtc], [OM_SystemCreateUser], [OM_SystemLastEditTimeUtc], [OM_SystemLastEditUser])
VALUES (@pk, @orgHeaderPk, @acheivableClientRevenue, @consultingRevenue, @estimatedProfit, @noOfEmployees, @paidUpCapital, @percentage, @totalClientRevenue, @warehouseRevenue, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgMiscServWithCM(Guid orgHeaderPk, decimal acheivableClientRevenue, decimal consultingRevenue, decimal estimatedProfit, int noOfEmployees, decimal paidUpCapital, decimal percentage, decimal totalClientRevenue, decimal warehouseRevenue)
		{
			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command(CreateOrgMiscServWithCMSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, OrgMiscServSchema.PK);
				command.AddParameterBasedOnDbColumn("@orgHeaderPk", orgHeaderPk, OrgMiscServSchema.OM_OH);
				command.AddParameterBasedOnDbColumn("@acheivableClientRevenue", acheivableClientRevenue, OrgMiscServSchema.OM_CMAcheivableClientRevenue);
				command.AddParameterBasedOnDbColumn("@consultingRevenue", consultingRevenue, OrgMiscServSchema.OM_CMConsultingRevenue);
				command.AddParameterBasedOnDbColumn("@estimatedProfit", estimatedProfit, OrgMiscServSchema.OM_CMEstimatedProfit);
				command.AddParameterBasedOnDbColumn("@noOfEmployees", noOfEmployees, OrgMiscServSchema.OM_CMNoOfEmployees);
				command.AddParameterBasedOnDbColumn("@paidUpCapital", paidUpCapital, OrgMiscServSchema.OM_CMPaidUpCapital);
				command.AddParameterBasedOnDbColumn("@percentage", percentage, OrgMiscServSchema.OM_CMPercentage);
				command.AddParameterBasedOnDbColumn("@totalClientRevenue", totalClientRevenue, OrgMiscServSchema.OM_CMTotalClientRevenue);
				command.AddParameterBasedOnDbColumn("@warehouseRevenue", warehouseRevenue, OrgMiscServSchema.OM_CMWarehouseRevenue);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		#region CreateOrgMiscSer

		const string CreateOrgMiscSerSql = @"
INSERT INTO [dbo].[OrgMiscServ] ([OM_PK], [OM_IMImporterCategory], [OM_IMAutoPopulateOwnerRefWithOrderNums], [OM_IMDefaultWarehousePickOption], [OM_IMInvoiceDetailReportSort], [OM_CMCompetitorActivity], [OM_OH], [OM_IMInvoiceDetailReportSort2], [OM_IMInvoiceDetailReportSort3], [OM_EXMergeCustomsInvoiceLinesBy], [OM_WhsOrderFulfillmentRule], [OM_WhsPackingSlipOrderBy], [OM_WhsDefaultWarehousePickMode], [OM_WhsTransportPayer], [OM_WhsABCAnalysisMethod], [OM_WhsABCAnalysisPeriod], [OM_ConsigneeAuthorityToLeave] ,[OM_ConsignorAuthorityToLeave], [OM_CMAuthorityToLeave], [OM_SystemCreateTimeUtc], [OM_SystemCreateUser], [OM_SystemLastEditTimeUtc], [OM_SystemLastEditUser])
VALUES (@pk, @importerCategory, @ownerRef, @pickOption, @reportSort, @competitorActivity, @orgHeaderPk, @reportSort2, @reportSort3, @invoiceLinesBy, @fulfillmentRule, @packingSlipOrderBy, @pickMode, @transportPayer, @analysisMethod, @analysisPeriod, @consigneeAuthority, @consignorAuthority, @authority, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgMiscSer(string importerCategory, Guid orgHeaderPk)
		{
			var pk = Guid.NewGuid();
			const string def = "DEF";
			const string aut = "AUT";
			const string frt = "FRT";
			const string non = "NON";
			const string asp = "ASP";

			using (var command = Db.Connection.Command(CreateOrgMiscSerSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@importerCategory", SqlDbType.VarChar, OrgMiscServSchema.OM_IMImporterCategory.MaxLength, importerCategory);
				command.AddParameter("@ownerRef", SqlDbType.VarChar, OrgMiscServSchema.OM_IMAutoPopulateOwnerRefWithOrderNums.MaxLength, def);
				command.AddParameter("@pickOption", SqlDbType.VarChar, OrgMiscServSchema.OM_IMDefaultWarehousePickOption.MaxLength, aut);
				command.AddParameter("@reportSort", SqlDbType.VarChar, OrgMiscServSchema.OM_IMInvoiceDetailReportSort.MaxLength, def);
				command.AddParameter("@competitorActivity", SqlDbType.Char, OrgMiscServSchema.OM_CMCompetitorActivity.MaxLength, frt);
				command.AddParameter("@orgHeaderPk", SqlDbType.UniqueIdentifier, orgHeaderPk);
				command.AddParameter("@reportSort2", SqlDbType.VarChar, OrgMiscServSchema.OM_IMInvoiceDetailReportSort2.MaxLength, def);
				command.AddParameter("@reportSort3", SqlDbType.VarChar, OrgMiscServSchema.OM_IMInvoiceDetailReportSort3.MaxLength, def);
				command.AddParameter("@invoiceLinesBy", SqlDbType.VarChar, OrgMiscServSchema.OM_EXMergeCustomsInvoiceLinesBy.MaxLength, non);
				command.AddParameter("@fulfillmentRule", SqlDbType.VarChar, OrgMiscServSchema.OM_WhsOrderFulfillmentRule.MaxLength, non);
				command.AddParameter("@packingSlipOrderBy", SqlDbType.VarChar, OrgMiscServSchema.OM_WhsPackingSlipOrderBy.MaxLength, def);
				command.AddParameter("@pickMode", SqlDbType.VarChar, OrgMiscServSchema.OM_WhsDefaultWarehousePickMode.MaxLength, asp);
				command.AddParameter("@transportPayer", SqlDbType.VarChar, OrgMiscServSchema.OM_WhsTransportPayer.MaxLength, def);
				command.AddParameter("@analysisMethod", SqlDbType.Char, OrgMiscServSchema.OM_WhsABCAnalysisMethod.MaxLength, def);
				command.AddParameter("@analysisPeriod", SqlDbType.Char, OrgMiscServSchema.OM_WhsABCAnalysisPeriod.MaxLength, def);
				command.AddParameter("@consigneeAuthority", SqlDbType.VarChar, OrgMiscServSchema.OM_ConsigneeAuthorityToLeave.MaxLength, def);
				command.AddParameter("@consignorAuthority", SqlDbType.VarChar, OrgMiscServSchema.OM_ConsignorAuthorityToLeave.MaxLength, def);
				command.AddParameter("@authority", SqlDbType.VarChar, OrgMiscServSchema.OM_CMAuthorityToLeave.MaxLength, def);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgWebURL

		const string CreateOrgWebUrlSql = @"
INSERT INTO [dbo].[OrgWebURL] ([PU_PK], [PU_Type], [PU_OH], [PU_Description], [PU_URL], [PU_SystemCreateTimeUtc], [PU_SystemCreateUser], [PU_SystemLastEditTimeUtc], [PU_SystemLastEditUser])
VALUES (@pk, @type, @orgHeaderPk, @description, @url, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgWebUrl(string description, string url, Guid orgHeaderPk)
		{
			var pk = Guid.NewGuid();
			const string type = "MAI";

			using (var command = Db.Connection.Command(CreateOrgWebUrlSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@type", SqlDbType.VarChar, OrgWebURLSchema.PU_Type.MaxLength, type);
				command.AddParameter("@orgHeaderPk", SqlDbType.UniqueIdentifier, orgHeaderPk);
				command.AddParameter("@description", SqlDbType.NVarChar, OrgWebURLSchema.PU_Description.MaxLength, description);
				command.AddParameter("@url", SqlDbType.NVarChar, OrgWebURLSchema.PU_URL.MaxLength, url);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgCustomLabels

		const string CreateOrgCustomLabelsSql = @"
INSERT INTO [dbo].[OrgCustomLabels] ([OT_PK], [OT_Type], [OT_FieldName], [OT_Caption], [OT_OH], [OT_SystemCreateTimeUtc], [OT_SystemCreateUser], [OT_SystemLastEditTimeUtc], [OT_SystemLastEditUser])
VALUES (@pk, @type, @fieldName, @caption, @orgHeaderPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgCustomLabels(string fieldName, string caption, Guid orgHeaderPk)
		{
			var pk = Guid.NewGuid();
			const string type = "FRM";

			using (var command = Db.Connection.Command(CreateOrgCustomLabelsSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@type", SqlDbType.VarChar, OrgCustomLabelsSchema.OT_Type.MaxLength, type);
				command.AddParameter("@fieldName", SqlDbType.VarChar, OrgCustomLabelsSchema.OT_FieldName.MaxLength, fieldName);
				command.AddParameter("@caption", SqlDbType.VarChar, OrgCustomLabelsSchema.OT_Caption.MaxLength, caption);
				command.AddParameter("@orgHeaderPk", SqlDbType.UniqueIdentifier, orgHeaderPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefAirline

		const string CreateRefAirlineSql = @"
INSERT INTO [dbo].[RefAirline] ([RM_PK], [RM_AirlineName1], [RM_TwoCharacterCode], [RM_AddressLine1], [RM_AirlineCity], [RM_AirlineCountry], [RM_TypeOfOperationsCode], [RM_SystemCreateTimeUtc], [RM_SystemCreateUser], [RM_SystemLastEditTimeUtc], [RM_SystemLastEditUser])
VALUES (@pk, @airlineName, @code, @addressLine, @city, @country, @type, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefAirline(string airlineName, string code, string addressLine, string city, string country, string type)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefAirlineSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@airlineName", SqlDbType.VarChar, RefAirlineSchema.RM_AirlineName1.MaxLength, airlineName);
				command.AddParameter("@code", SqlDbType.VarChar, RefAirlineSchema.RM_TwoCharacterCode.MaxLength, code);
				command.AddParameter("@addressLine", SqlDbType.VarChar, RefAirlineSchema.RM_AddressLine1.MaxLength, addressLine);
				command.AddParameter("@city", SqlDbType.VarChar, RefAirlineSchema.RM_AirlineCity.MaxLength, city);
				command.AddParameter("@country", SqlDbType.VarChar, RefAirlineSchema.RM_AirlineCountry.MaxLength, country);
				command.AddParameter("@type", SqlDbType.VarChar, RefAirlineSchema.RM_TypeOfOperationsCode.MaxLength, type);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefCarrierConsortium

		const string CreateRefCarrierConsortiumSql = @"
INSERT INTO [dbo].[RefCarrierConsortium] ([RG_PK],[RG_Code], [RG_OH], [RG_SystemCreateTimeUtc], [RG_SystemCreateUser], [RG_SystemLastEditTimeUtc], [RG_SystemLastEditUser])
VALUES (@pk, @code, @orgHeaderPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefCarrierConsortium(string code, Guid orgHeaderPk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefCarrierConsortiumSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, RefCarrierConsortiumSchema.RG_Code.MaxLength, code);
				command.AddParameter("@orgHeaderPk", SqlDbType.UniqueIdentifier, orgHeaderPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefCityTown

		const string CreateRefCityTownSql = @"
INSERT INTO [dbo].[RefCityTown] ([R9_PK], [R9_InternationalName], [R9_RW_NKState], [R9_RN_NKCountry], [R9_SystemCreateTimeUtc], [R9_SystemCreateUser], [R9_SystemLastEditTimeUtc], [R9_SystemLastEditUser])
VALUES (@pk, @internationalName, @state, @country, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefCityTown(string internationalName)
		{
			var pk = Guid.NewGuid();
			const string state = "NSW";
			const string country = "AU";

			using (var command = Db.Connection.Command(CreateRefCityTownSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@internationalName", SqlDbType.VarChar, RefCityTownSchema.R9_InternationalName.MaxLength, internationalName);
				command.AddParameter("@state", SqlDbType.VarChar, RefCityTownSchema.R9_RW_NKState.MaxLength, state);
				command.AddParameter("@country", SqlDbType.Char, RefCityTownSchema.R9_RN_NKCountry.MaxLength, country);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefCommodityCode

		const string CreateRefCommodityCodeSql = @"
INSERT INTO [dbo].[RefCommodityCode] ([RH_PK], [RH_Code], [RH_Description], [RH_SystemCreateTimeUtc], [RH_SystemCreateUser], [RH_SystemLastEditTimeUtc], [RH_SystemLastEditUser])
VALUES (@pk, @code, @description, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefCommodityCode(string code, string description)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefCommodityCodeSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, RefCommodityCodeSchema.RH_Code.MaxLength, code);
				command.AddParameter("@description", SqlDbType.VarChar, RefCommodityCodeSchema.RH_Description.MaxLength, description);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefComplianceList

		const string CreateRefComplianceListSql = @"INSERT INTO [dbo].[RefComplianceList] ([RCL_PK], [RCL_ListCode], [RCL_ListName], [RCL_ListDescription], [RCL_ListPublisher], [RCL_ListType], [RCL_PublisherJurisdiction], [RCL_LastUpdatedDate]) VALUES (@pk, @listCode, @listName, @listDescription, @listPublisher, @listType, @publisherJurisdiction, @lastUpdatedDate)";

		public Guid CreateRefComplianceList(string listCode, string listName, string listDescription)
		{
			var pk = Guid.NewGuid();
			const string listType = "Financial Sanctions";
			const string listPublisher = "European Union";
			const string publisherJurisdiction = "Europe";
			const string lastUpdatedDate = "2021-06-30";

			using (var command = Db.Connection.Command(CreateRefComplianceListSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@listCode", SqlDbType.VarChar, RefComplianceListSchema.RCL_ListCode.MaxLength, listCode);
				command.AddParameter("@listName", SqlDbType.NVarChar, RefComplianceListSchema.RCL_ListName.MaxLength, listName);
				command.AddParameter("@listDescription", SqlDbType.NVarChar, listDescription);
				command.AddParameter("@listPublisher", SqlDbType.NVarChar, RefComplianceListSchema.RCL_ListPublisher.MaxLength, listPublisher);
				command.AddParameter("@listType", SqlDbType.VarChar, RefComplianceListSchema.RCL_ListType.MaxLength, listType);
				command.AddParameter("@publisherJurisdiction", SqlDbType.VarChar, RefComplianceListSchema.RCL_PublisherJurisdiction.MaxLength, publisherJurisdiction);
				command.AddParameter("@lastUpdatedDate", SqlDbType.Date, RefComplianceListSchema.RCL_LastUpdatedDate.MaxLength, lastUpdatedDate);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefContainer

		const string CreateRefContainerSql = @"
INSERT INTO [dbo].[RefContainer] ([RC_PK], [RC_Code], [RC_ShippingMode], [RC_Description], [RC_ContainerType], [RC_SystemCreateTimeUtc], [RC_SystemCreateUser], [RC_SystemLastEditTimeUtc], [RC_SystemLastEditUser])
VALUES (@pk, @code, @shippingMode, @description, @containerType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefContainer(string code, string containerType, string transportMode = null)
		{
			var pk = Guid.NewGuid();
			const string description = "TANK CONT";

			using (var command = Db.Connection.Command(CreateRefContainerSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, RefContainerSchema.RC_Code.MaxLength, code);
				command.AddParameter("@shippingMode", SqlDbType.VarChar, RefContainerSchema.RC_ShippingMode.MaxLength, transportMode ?? "SEA");
				command.AddParameter("@description", SqlDbType.VarChar, RefContainerSchema.RC_Description.MaxLength, description);
				command.AddParameter("@containerType", SqlDbType.VarChar, RefContainerSchema.RC_ContainerType.MaxLength, containerType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefCountry

		const string CreateRefCountrySql = @"
INSERT INTO [dbo].[RefCountry] ([RN_PK], [RN_Code], [RN_Desc], [RN_AddressFormattingRule], [RN_PostcodeValidationRule], [RN_StateProvinceValidationRule], [RN_RX_NKLocalCurrency], [RN_ValidationStatus], [RN_SystemCreateTimeUtc], [RN_SystemCreateUser], [RN_SystemLastEditTimeUtc], [RN_SystemLastEditUser])
VALUES (@pk, @code, @description, @addressFormattingRule, @postcodeValidationRule, @stateProvinceValidationRule, @currency, @validationStatus, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefCountry(string code, string description, string currency)
		{
			var pk = Guid.NewGuid();
			const string noValidationRule = "NVR";
			const string defaultRule = "DEF";
			const string validationStatus = "NAV";

			using (var command = Db.Connection.Command(CreateRefCountrySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.Char, RefCountrySchema.RN_Code.MaxLength, code);
				command.AddParameter("@description", SqlDbType.VarChar, RefCountrySchema.RN_Desc.MaxLength, description);
				command.AddParameter("@addressFormattingRule", SqlDbType.VarChar, RefCountrySchema.RN_AddressFormattingRule.MaxLength, defaultRule);
				command.AddParameter("@postcodeValidationRule", SqlDbType.VarChar, RefCountrySchema.RN_PostcodeValidationRule.MaxLength, noValidationRule);
				command.AddParameter("@stateProvinceValidationRule", SqlDbType.VarChar, RefCountrySchema.RN_StateProvinceValidationRule.MaxLength, noValidationRule);
				command.AddParameter("@currency", SqlDbType.VarChar, RefCountrySchema.RN_RX_NKLocalCurrency.MaxLength, currency);
				command.AddParameter("@validationStatus", SqlDbType.Char, RefCountrySchema.RN_ValidationStatus.MaxLength, validationStatus);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefCountryStates

		const string CreateRefCountryStatesSql = @"
INSERT INTO [dbo].[RefCountryStates] ([RW_PK], [RW_Code], [RW_RN_NKCountryCode], [RW_Description], [RW_IsActive], [RW_SystemCreateTimeUtc], [RW_SystemCreateUser], [RW_SystemLastEditTimeUtc], [RW_SystemLastEditUser])
VALUES (@pk, @code, @countryCode, @description, @isActive, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefCountryStates(string code, string countryCode, string description, bool isActive = true)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefCountryStatesSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, RefCountryStatesSchema.RW_Code.MaxLength, code);
				command.AddParameter("@countryCode", SqlDbType.Char, RefCountryStatesSchema.RW_RN_NKCountryCode.MaxLength, countryCode);
				command.AddParameter("@description", SqlDbType.NVarChar, RefCountryStatesSchema.RW_Description.MaxLength, description);
				command.AddParameter("@isActive", SqlDbType.Bit, RefCountryStatesSchema.RW_IsActive.MaxLength, isActive);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefCurrency

		const string CreateRefCurrencySql = @"
INSERT INTO [dbo].[RefCurrency] ([RX_PK], [RX_Code], [RX_Symbol], [RX_Desc], [RX_UnitName], [RX_SubUnitName], [RX_ISOSubUnitRatio], [RX_SystemCreateTimeUtc], [RX_SystemCreateUser], [RX_SystemLastEditTimeUtc], [RX_SystemLastEditUser])
VALUES (@pk, @code, @symbol, @description, @unitName, @subUnitName, @isoSubUnitRatio, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefCurrency(string code, string symbol, string description, string unitName, string subUnitName)
		{
			var pk = Guid.NewGuid();
			var isoSubUnitRatio = 100;

			using (var command = Db.Connection.Command(CreateRefCurrencySql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.Char, RefCurrencySchema.RX_Code.MaxLength, code);
				command.AddParameter("@symbol", SqlDbType.NVarChar, RefCurrencySchema.RX_Symbol.MaxLength, symbol);
				command.AddParameter("@description", SqlDbType.NVarChar, RefCurrencySchema.RX_Desc.MaxLength, description);
				command.AddParameter("@unitName", SqlDbType.VarChar, RefCurrencySchema.RX_UnitName.MaxLength, unitName);
				command.AddParameter("@subUnitName", SqlDbType.VarChar, RefCurrencySchema.RX_SubUnitName.MaxLength, subUnitName);
				command.AddParameter("@isoSubUnitRatio", SqlDbType.Int, isoSubUnitRatio);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefDocSource

		const string CreateRefDocSourceSql = @"
INSERT INTO [dbo].[RefDocSource] ([RDS_PK], [RDS_Code], [RDS_Desc], [RDS_SystemCreateTimeUtc], [RDS_SystemCreateUser], [RDS_SystemLastEditTimeUtc], [RDS_SystemLastEditUser])
VALUES (@pk, @code, @description, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefDocSource(string code, string description)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefDocSourceSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, RefDocSourceSchema.RDS_Code.MaxLength, code);
				command.AddParameter("@description", SqlDbType.VarChar, RefDocSourceSchema.RDS_Desc.MaxLength, description);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefDocType

		const string CreateRefDocTypeSql = @"
INSERT INTO [dbo].[RefDocType] ([RT_PK], [RT_ReferenceType], [RT_DocType], [RT_Desc], [RT_SystemCreateTimeUtc], [RT_SystemCreateUser], [RT_SystemLastEditTimeUtc], [RT_SystemLastEditUser])
VALUES (@pk, @referenceType, @docType, @description, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefDocType(string referenceType, string docType, string description)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefDocTypeSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@referenceType", SqlDbType.VarChar, RefDocTypeSchema.RT_ReferenceType.MaxLength, referenceType);
				command.AddParameter("@docType", SqlDbType.VarChar, RefDocTypeSchema.RT_DocType.MaxLength, docType);
				command.AddParameter("@description", SqlDbType.VarChar, RefDocTypeSchema.RT_Desc.MaxLength, description);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefNMFC

		const string CreateRefNMFCSql = @"
INSERT INTO [dbo].[RefNMFC] ([FN_PK], [FN_Code], [FN_ItemNo], [FN_Class], [FN_Description], [FN_SystemCreateTimeUtc], [FN_SystemCreateUser], [FN_SystemLastEditTimeUtc], [FN_SystemLastEditUser])
VALUES (@pk, @code, @itemNo, @classNo, @description, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefNMFC(string code, string itemNo, string classNo, string description)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefNMFCSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, RefNMFCSchema.FN_Code.MaxLength, code);
				command.AddParameter("@itemNo", SqlDbType.VarChar, RefNMFCSchema.FN_ItemNo.MaxLength, itemNo);
				command.AddParameter("@classNo", SqlDbType.VarChar, RefNMFCSchema.FN_Class.MaxLength, classNo);
				command.AddParameter("@description", SqlDbType.VarChar, RefNMFCSchema.FN_Description.MaxLength, description);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefPackType

		const string CreateRefPackTypeSql = @"
INSERT INTO [dbo].[RefPackType] ([F3_PK], [F3_Code], [F3_Description], [F3_SystemCreateTimeUtc], [F3_SystemCreateUser], [F3_SystemLastEditTimeUtc], [F3_SystemLastEditUser])
VALUES (@pk, @code, @description, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefPackType(string code, string description)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefPackTypeSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, RefPackTypeSchema.F3_Code.MaxLength, code);
				command.AddParameter("@description", SqlDbType.VarChar, RefPackTypeSchema.F3_Description.MaxLength, description);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefPacks

		const string CreateRefPacksSql = @"
INSERT INTO [dbo].[RefPacks] ([RP_PK], [RP_CommercialPack], [RP_CustomsPack], [RP_CustomsCountry], [RP_SystemCreateTimeUtc], [RP_SystemCreateUser], [RP_SystemLastEditTimeUtc], [RP_SystemLastEditUser])
VALUES (@pk, @commercialPack, @customsPack, @customsCountry, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefPacks(string commercialPack, string customsPack, string customsCountry)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefPacksSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@commercialPack", SqlDbType.VarChar, RefPacksSchema.RP_CommercialPack.MaxLength, commercialPack);
				command.AddParameter("@customsPack", SqlDbType.VarChar, RefPacksSchema.RP_CustomsPack.MaxLength, customsPack);
				command.AddParameter("@customsCountry", SqlDbType.VarChar, RefPacksSchema.RP_CustomsCountry.MaxLength, customsCountry);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefPostCode

		const string CreateRefPostCodeSql = @"
INSERT INTO [dbo].[RefPostCode] ([RK_PK], [RK_CityTownPostCode], [RK_RN_NKCountry], [RK_SystemCreateTimeUtc], [RK_SystemCreateUser], [RK_SystemLastEditTimeUtc], [RK_SystemLastEditUser])
VALUES (@pk, @postCode, @country, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefPostCode(string postCode, string country)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefPostCodeSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@postCode", SqlDbType.VarChar, RefPostCodeSchema.RK_CityTownPostCode.MaxLength, postCode);
				command.AddParameter("@country", SqlDbType.Char, RefPostCodeSchema.RK_RN_NKCountry.MaxLength, country);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefPremisesGateCode

		const string CreateRefPremisesGateCodeSql = @"
INSERT INTO [dbo].[RefPremisesGateCode] ([R5_PK], [R5_DataProvider], [R5_OrgRegCodeType], [R5_PremisesGateCode], [R5_PremisesGateDescription], [R5_SystemCreateTimeUtc], [R5_SystemCreateUser], [R5_SystemLastEditTimeUtc], [R5_SystemLastEditUser])
VALUES (@pk, @dataProvider, @codeType, @gateCode, @gateDescription, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefPremisesGateCode(string dataProvider, string codeType, string gateCode, string gateDescription)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefPremisesGateCodeSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@dataProvider", SqlDbType.VarChar, RefPremisesGateCodeSchema.R5_DataProvider.MaxLength, dataProvider);
				command.AddParameter("@codeType", SqlDbType.VarChar, RefPremisesGateCodeSchema.R5_OrgRegCodeType.MaxLength, codeType);
				command.AddParameter("@gateCode", SqlDbType.VarChar, RefPremisesGateCodeSchema.R5_PremisesGateCode.MaxLength, gateCode);
				command.AddParameter("@gateDescription", SqlDbType.VarChar, RefPremisesGateCodeSchema.R5_PremisesGateDescription.MaxLength, gateDescription);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefServiceLevel

		const string CreateRefServiceLevelSql = @"
INSERT INTO [dbo].[RefServiceLevel] ([RS_PK], [RS_Code], [RS_Description], [RS_ServiceDeliveryType], [RS_SystemCreateTimeUtc], [RS_SystemCreateUser], [RS_SystemLastEditTimeUtc], [RS_SystemLastEditUser])
VALUES (@pk, @code, @description, @deliveryType, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefServiceLevel(string code, string description, string deliveryType)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefServiceLevelSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.Char, RefServiceLevelSchema.RS_Code.MaxLength, code);
				command.AddParameter("@description", SqlDbType.VarChar, RefServiceLevelSchema.RS_Description.MaxLength, description);
				command.AddParameter("@deliveryType", SqlDbType.VarChar, RefServiceLevelSchema.RS_ServiceDeliveryType.MaxLength, deliveryType);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefShippingLine

		const string CreateRefShippingLineSql = @"
INSERT INTO [dbo].[RefShippingLine] ([RSL_PK], [RSL_CarrierName], [RSL_StandardCarrierAlphaCode], [RSL_CargoWiseOneCode], [RSL_SystemCreateTimeUtc], [RSL_SystemCreateUser], [RSL_SystemLastEditTimeUtc], [RSL_SystemLastEditUser])
VALUES (@pk, @carrierName, @standardCarrierAlphaCode, @cargoWiseOneCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefShippingLine(string carrierName, string standardCarrierAlphaCode, string cargoWiseOneCode)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefShippingLineSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@carrierName", SqlDbType.NVarChar, RefShippingLineSchema.RSL_CarrierName.MaxLength, carrierName);
				command.AddParameter("@standardCarrierAlphaCode", SqlDbType.VarChar, RefShippingLineSchema.RSL_StandardCarrierAlphaCode.MaxLength, standardCarrierAlphaCode);
				command.AddParameter("@cargoWiseOneCode", SqlDbType.VarChar, RefShippingLineSchema.RSL_CargoWiseOneCode.MaxLength, cargoWiseOneCode);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefTimeZone

		const string CreateRefTimeZoneSql = @"
INSERT INTO [dbo].[RefTimeZone] ([R2_PK], [R2_CivilianTimeZoneCode], [R2_OffsetMinutesFromUTC], [R2_SystemCreateTimeUtc], [R2_SystemCreateUser], [R2_SystemLastEditTimeUtc], [R2_SystemLastEditUser])
VALUES (@pk, @civilianTimeZoneCode, @offsetMinutesFromUtc, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefTimeZone(string civilianTimeZoneCode, short offsetMinutesFromUtc)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefTimeZoneSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@civilianTimeZoneCode", SqlDbType.VarChar, RefTimeZoneSchema.R2_CivilianTimeZoneCode.MaxLength, civilianTimeZoneCode);
				command.AddParameter("@offsetMinutesFromUtc", SqlDbType.SmallInt, RefTimeZoneSchema.R2_OffsetMinutesFromUTC.MaxLength, offsetMinutesFromUtc);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefTimeZoneSet

		const string CreateRefTimeZoneSetSql = @"
INSERT INTO [dbo].[RefTimeZoneSet] ([R3_PK], [R3_TimeZoneSetName], [R3_R2_StandardZone], [R3_SystemCreateTimeUtc], [R3_SystemCreateUser], [R3_SystemLastEditTimeUtc], [R3_SystemLastEditUser])
VALUES (@pk, @name, @standardZonePk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefTimeZoneSet(string name, Guid standardZonePk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefTimeZoneSetSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@name", SqlDbType.VarChar, RefTimeZoneSetSchema.R3_TimeZoneSetName.MaxLength, name);
				command.AddParameter("@standardZonePk", SqlDbType.UniqueIdentifier, standardZonePk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefTransitTime

		const string CreateRefTransitTimeSql = @"INSERT INTO [dbo].[RefTransitTime] ([RTT_PK], [RTT_RS_NKServiceLevel], [RTT_TransitHours], [RTT_TZ_OriginDomesticZone], [RTT_TZ_DestinationDomesticZone], [RTT_SystemCreateTimeUtc], [RTT_SystemCreateUser], [RTT_SystemLastEditTimeUtc], [RTT_SystemLastEditUser]) VALUES (@pk, @serviceLevel, @transitHours, @originZonePk, @destinationZonePk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefTransitTimeSet(string serviceLevel, int transitHours, Guid originDomesticZonePk, Guid destinationDomesticZonePk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefTransitTimeSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@serviceLevel", SqlDbType.VarChar, RefTransitTimeSchema.RTT_RS_NKServiceLevel.MaxLength, serviceLevel);
				command.AddParameter("@transitHours", SqlDbType.Int, transitHours);
				command.AddParameter("@originZonePk", SqlDbType.UniqueIdentifier, originDomesticZonePk);
				command.AddParameter("@destinationZonePk", SqlDbType.UniqueIdentifier, destinationDomesticZonePk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRateTransportZone

		public Guid CreateRateTransportZone(string zoneName, string zoneType, string zoneMode)
		{
			var insertRateTransportZoneSql = @"INSERT INTO [dbo].[RateTransportZones] ([TZ_PK], [TZ_TP], [TZ_ZoneName], [TZ_SystemCreateTimeUtc], [TZ_SystemCreateUser], [TZ_SystemLastEditTimeUtc], [TZ_SystemLastEditUser]) VALUES (@pk, @rateTransportProviderPk, @zoneName, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			var rateTransportProviderPk = CreateRateTransportProvider(zoneType, zoneMode);
			var guid = Guid.NewGuid();
			using (var dbCommand = Db.Connection.Command(insertRateTransportZoneSql))
			{
				dbCommand.AddParameter("@pk", SqlDbType.UniqueIdentifier, guid);
				dbCommand.AddParameter("@rateTransportProviderPk", SqlDbType.UniqueIdentifier, rateTransportProviderPk);
				dbCommand.AddParameter("@zoneName", SqlDbType.VarChar, RateTransportZonesSchema.TZ_ZoneName.MaxLength, zoneName);
				dbCommand.ExecuteNonQuery();
			}

			return guid;
		}

		#endregion

		#region CreateRateTransportProvider

		public Guid CreateRateTransportProvider(string zoneType, string zoneMode)
		{
			var insertRateTransportProviderSql = @"INSERT INTO [dbo].[RateTransportProvider] ([TP_PK], [TP_ZoneType], [TP_ZoneMode], [TP_RN_NKCountry], [TP_SystemCreateTimeUtc], [TP_SystemCreateUser], [TP_SystemLastEditTimeUtc], [TP_SystemLastEditUser]) VALUES (@pk, @zoneType, @zoneMode, 'AU', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			var guid = Guid.NewGuid();
			using (var dbCommand = Db.Connection.Command(insertRateTransportProviderSql))
			{
				dbCommand.AddParameter("@pk", SqlDbType.UniqueIdentifier, guid);
				dbCommand.AddParameter("@zoneType", SqlDbType.VarChar, RateTransportProviderSchema.TP_ZoneType.MaxLength, zoneType);
				dbCommand.AddParameter("@zoneMode", SqlDbType.VarChar, RateTransportProviderSchema.TP_ZoneMode.MaxLength, zoneMode);
				dbCommand.ExecuteNonQuery();
			}

			return guid;
		}

		#endregion

		#region CreateRefZoneHeader

		const string CreateRefZoneHeaderSql = @"INSERT INTO [dbo].[RefZoneHeader] ([FZ_PK], [FZ_Code], [FZ_Description], [FZ_ZoneType], [FZ_SystemCreateTimeUtc], [FZ_SystemCreateUser], [FZ_SystemLastEditTimeUtc], [FZ_SystemLastEditUser]) VALUES (@pk, @code, @description, @zoneType, @systemTime, @systemUser, @systemTime, @systemUser)";

		public Guid CreateRefZoneHeader(string code, string description, string zoneType)
		{
			var pk = Guid.NewGuid();
			var systemTime = DateTime.UtcNow;
			var systemUser = "~BP";

			using (var command = Db.Connection.Command(CreateRefZoneHeaderSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, RefZoneHeaderSchema.FZ_Code.MaxLength, code);
				command.AddParameter("@description", SqlDbType.VarChar, RefZoneHeaderSchema.FZ_Description.MaxLength, description);
				command.AddParameter("@zoneType", SqlDbType.Char, RefZoneHeaderSchema.FZ_ZoneType.MaxLength, zoneType);
				command.AddParameter("@systemTime", SqlDbType.DateTime, RefZoneHeaderSchema.FZ_SystemCreateTimeUtc.MaxLength, systemTime);
				command.AddParameter("@systemUser", SqlDbType.VarChar, RefZoneHeaderSchema.FZ_SystemCreateUser.MaxLength, systemUser);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefUNLOCO

		const string CreateRefUNLOCOSql = @"
INSERT INTO [dbo].[RefUNLOCO] ([RL_PK], [RL_Code], [RL_PortName], [RL_NameWithDiacriticals], [RL_R3], [RL_RN_NKCountryCode], [RL_RW], [RL_SystemCreateTimeUtc], [RL_SystemCreateUser], [RL_SystemLastEditTimeUtc], [RL_SystemLastEditUser])
VALUES (@pk, @code, @portName, @nameWithDiacriticals, @timeZoneSetPk, @countryCode, @countyStatesPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefUNLOCO(string code, string portName, string nameWithDiacriticals, string countryCode, Guid timeZoneSetPk, Guid countyStatesPk)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRefUNLOCOSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, RefUNLOCOSchema.RL_Code.MaxLength, code);
				command.AddParameter("@portName", SqlDbType.VarChar, RefUNLOCOSchema.RL_PortName.MaxLength, portName);
				command.AddParameter("@nameWithDiacriticals", SqlDbType.NVarChar, RefUNLOCOSchema.RL_NameWithDiacriticals.MaxLength, nameWithDiacriticals);
				command.AddParameter("@timeZoneSetPk", SqlDbType.UniqueIdentifier, timeZoneSetPk);
				command.AddParameter("@countryCode", SqlDbType.VarChar, RefUNLOCOSchema.RL_RN_NKCountryCode.MaxLength, countryCode);
				command.AddParameter("@countyStatesPk", SqlDbType.UniqueIdentifier, countyStatesPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefVessel

		const string CreateRefVesselSql = @"
INSERT INTO [dbo].[RefVessel] ([RV_PK], [RV_Code], [RV_LloydsNumber], [RV_ScreeningStatus], [RV_SystemCreateTimeUtc], [RV_SystemCreateUser], [RV_SystemLastEditTimeUtc], [RV_SystemLastEditUser])
VALUES (@pk, @code, @lloydsNumber, @screeningStatus, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRefVessel(string code, string lloydsNumber)
		{
			var pk = Guid.NewGuid();
			const string screeningStatus = "UNK";

			using (var command = Db.Connection.Command(CreateRefVesselSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@code", SqlDbType.VarChar, RefVesselSchema.RV_Code.MaxLength, code);
				command.AddParameter("@lloydsNumber", SqlDbType.Char, RefVesselSchema.RV_LloydsNumber.MaxLength, lloydsNumber);
				command.AddParameter("@screeningStatus", SqlDbType.Char, RefVesselSchema.RV_ScreeningStatus.MaxLength, screeningStatus);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRelatedActivityPivot

		const string CreateRelatedActivityPivotSql = @"INSERT INTO dbo.RelatedActivityPivot (RAP_PK, RAP_ChildActivityID, RAP_ChildActivityTableCode, RAP_ParentActivityID, RAP_ParentActivityTableCode, RAP_SalesRelationTreeID, RAP_SystemCreateTimeUtc, RAP_SystemCreateUser, RAP_SystemLastEditTimeUtc, RAP_SystemLastEditUser) VALUES (@pk, @childID, @childTableCode, @parentID, @parentTableCode, @treeID, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateRelatedActivityPivot(Guid childID, string childTableCode, Guid parentID, string parentTableCode, Guid treeID = default)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateRelatedActivityPivotSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, RelatedActivityPivotSchema.PK);
				command.AddParameterBasedOnDbColumn("@childID", childID, RelatedActivityPivotSchema.RAP_ChildActivityID);
				command.AddParameterBasedOnDbColumn("@childTableCode", childTableCode, RelatedActivityPivotSchema.RAP_ChildActivityTableCode);
				command.AddParameterBasedOnDbColumn("@parentID", parentID, RelatedActivityPivotSchema.RAP_ParentActivityID);
				command.AddParameterBasedOnDbColumn("@parentTableCode", parentTableCode, RelatedActivityPivotSchema.RAP_ParentActivityTableCode);
				command.AddParameterBasedOnDbColumn("@treeID", treeID == default ? DBNull.Value : treeID, RelatedActivityPivotSchema.RAP_SalesRelationTreeID);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateStmEntityScreeningLog

		const string CreateStmEntityScreeningLogSql = @"
                INSERT INTO [dbo].[StmEntityScreeningLog] 
                ([PJ_PK], [PJ_ParentID], [PJ_ParentTableCode], [PJ_SourceID], [PJ_SourceTableCode], [PJ_SystemCreateTimeUtc], [PJ_SystemCreateUser], [PJ_SystemLastEditTimeUtc], [PJ_SystemLastEditUser], [PJ_Sequence])
                VALUES (@pk, @parentID, @parentTableCode, @sourceID, @sourceTableCode, @systemTime, @systemUser, @systemTime, @systemUser, @sequence)";
		public Guid CreateStmEntityScreeningLog(Guid? parentID, string parentTableCode, Guid? sourceID, string sourceTableCode, int sequence)
		{
			var pk = Guid.NewGuid();
			var systemTime = DateTime.UtcNow;
			var systemUser = "~BP";

			using (var command = Db.Connection.Command(CreateStmEntityScreeningLogSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentTableCode", SqlDbType.VarChar, StmEntityScreeningLogSchema.PJ_ParentTableCode.MaxLength, parentTableCode);
				command.AddParameter("@sourceTableCode", SqlDbType.VarChar, StmEntityScreeningLogSchema.PJ_SourceTableCode.MaxLength, sourceTableCode);
				command.AddParameter("@systemTime", SqlDbType.DateTime, StmEntityScreeningLogSchema.PJ_SystemCreateTimeUtc.MaxLength, systemTime);
				command.AddParameter("@systemUser", SqlDbType.VarChar, StmEntityScreeningLogSchema.PJ_SystemCreateUser.MaxLength, systemUser);
				command.AddParameter("@sequence", SqlDbType.Int, sequence);
				command.AddParameter("@parentID", SqlDbType.UniqueIdentifier, parentID != null ? parentID : DBNull.Value);
				command.AddParameter("@sourceID", SqlDbType.UniqueIdentifier, sourceID != null ? sourceID : DBNull.Value);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateUNDGCommonData

		const string CreateUNDGCommonDataSql = @"
INSERT INTO [dbo].[UNDGCommonData] ([DC_PK], [DC_Language], [DC_Type], [DC_Index], [DC_Descriptor], [DC_SystemCreateTimeUtc], [DC_SystemCreateUser], [DC_SystemLastEditTimeUtc], [DC_SystemLastEditUser])
VALUES (@pk, @language, @type, @index, @descriptor, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateUNDGCommonData(string index, string descriptor)
		{
			var pk = Guid.NewGuid();
			const string language = "ENG";
			const string type = "SPP";

			using (var command = Db.Connection.Command(CreateUNDGCommonDataSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@language", SqlDbType.VarChar, UNDGCommonDataSchema.DC_Language.MaxLength, language);
				command.AddParameter("@type", SqlDbType.VarChar, UNDGCommonDataSchema.DC_Type.MaxLength, type);
				command.AddParameter("@index", SqlDbType.VarChar, UNDGCommonDataSchema.DC_Index.MaxLength, index);
				command.AddParameter("@descriptor", SqlDbType.NVarChar, UNDGCommonDataSchema.DC_Descriptor.MaxLength, descriptor);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateVoteExamSurveyQuestion

		const string CreateVoteExamSurveyQuestionSql = @"
INSERT INTO [dbo].[VoteExamSurveyQuestion] ([HY_PK],[HY_QuestionOrder],[HY_SubQuestionOrder],[HY_AnswerType],[HY_ExamCorrectAnswer],[HY_Min],[HY_Max],[HY_G0],[HY_RN_NKCountryCode],[HY_QuestionCategory],[HY_AnswerWeighting],[HY_IsOptional],[HY_IsValid],
[HY_IsActive],[HY_IsRandomisable],[HY_OptionalAnswerExplanation],[HY_Question],[HY_Comment],[HY_SystemCreateTimeUtc], [HY_SystemCreateUser], [HY_SystemLastEditTimeUtc], [HY_SystemLastEditUser])
VALUES (@pk, @questionOrder, @subQuestionOrder, @answerType, @examCorrectAnswer, @min, @max, @campaignPk, @countryCode, @questionCategory, @answerWeighting, @isOptional, @isValid,
@isActive, @isRandomisable, @optionalAnswerExplanation, @question, @comment, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateVoteExamSurveyQuestion(Guid campaignPk, short questionOrder = 0, short subQuestionOrder = 0, string answerType = "", string examCorrectAnswer = "", byte min = 0, byte max = 0, string countryCode = "", string questionCategory = "", byte answerWeighting = 0, bool isOptional = true, bool isValid = true, bool isActive = true, bool isRandomisable = false, string optionalAnswerExplanation = "", string question = "", string comment = "")
		{
			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command(CreateVoteExamSurveyQuestionSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, VoteExamSurveyQuestionSchema.PK);
				command.AddParameterBasedOnDbColumn("@questionOrder", questionOrder, VoteExamSurveyQuestionSchema.HY_QuestionOrder);
				command.AddParameterBasedOnDbColumn("@subQuestionOrder", subQuestionOrder, VoteExamSurveyQuestionSchema.HY_SubQuestionOrder);
				command.AddParameterBasedOnDbColumn("@answerType", answerType, VoteExamSurveyQuestionSchema.HY_AnswerType);
				command.AddParameterBasedOnDbColumn("@examCorrectAnswer", examCorrectAnswer, VoteExamSurveyQuestionSchema.HY_ExamCorrectAnswer);
				command.AddParameterBasedOnDbColumn("@campaignPk", campaignPk, VoteExamSurveyQuestionSchema.HY_G0);
				command.AddParameterBasedOnDbColumn("@min", min, VoteExamSurveyQuestionSchema.HY_Min);
				command.AddParameterBasedOnDbColumn("@max", max, VoteExamSurveyQuestionSchema.HY_Max);
				command.AddParameterBasedOnDbColumn("@countryCode", countryCode, VoteExamSurveyQuestionSchema.HY_RN_NKCountryCode);
				command.AddParameterBasedOnDbColumn("@questionCategory", questionCategory, VoteExamSurveyQuestionSchema.HY_QuestionCategory);
				command.AddParameterBasedOnDbColumn("@answerWeighting", answerWeighting, VoteExamSurveyQuestionSchema.HY_AnswerWeighting);
				command.AddParameterBasedOnDbColumn("@isOptional", isOptional, VoteExamSurveyQuestionSchema.HY_IsOptional);
				command.AddParameterBasedOnDbColumn("@isValid", isValid, VoteExamSurveyQuestionSchema.HY_IsValid);
				command.AddParameterBasedOnDbColumn("@isActive", isActive, VoteExamSurveyQuestionSchema.HY_IsActive);
				command.AddParameterBasedOnDbColumn("@isRandomisable", isRandomisable, VoteExamSurveyQuestionSchema.HY_IsRandomisable);
				command.AddParameterBasedOnDbColumn("@optionalAnswerExplanation", optionalAnswerExplanation, VoteExamSurveyQuestionSchema.HY_OptionalAnswerExplanation);
				command.AddParameterBasedOnDbColumn("@question", question, VoteExamSurveyQuestionSchema.HY_Question);
				command.AddParameterBasedOnDbColumn("@comment", comment, VoteExamSurveyQuestionSchema.HY_Comment);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		#endregion

		#region CreateVoteExamSurveyAnswer

		const string CreateVoteExamSurveyAnswerSql = @"
INSERT INTO [dbo].[VoteExamSurveyAnswer] ([HZ_PK], [HZ_HY], [HZ_Answer], [HZ_G8], [HZ_IsValid], [HZ_AnswerComment], [HZ_QuestionOrder], [HZ_SubQuestionOrder], [HZ_SystemCreateTimeUtc], [HZ_SystemCreateUser], [HZ_SystemLastEditTimeUtc], [HZ_SystemLastEditUser])
VALUES (@pk, @questionPk, @answer, @campaignItemPk, @isValid, @answerComment, @questionOrder, @subQuestionOrder, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateVoteExamSurveyAnswer(Guid questionPk, Guid campaignItemPk, short questionOrder = 0, short subQuestionOrder = 0, string answer = "", bool isValid = true, string answerComment = "")
		{
			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command(CreateVoteExamSurveyAnswerSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, VoteExamSurveyAnswerSchema.PK);
				command.AddParameterBasedOnDbColumn("@questionPk", questionPk, VoteExamSurveyAnswerSchema.HZ_HY);
				command.AddParameterBasedOnDbColumn("@answer", answer, VoteExamSurveyAnswerSchema.HZ_Answer);
				command.AddParameterBasedOnDbColumn("@campaignItemPk", campaignItemPk, VoteExamSurveyAnswerSchema.HZ_G8);
				command.AddParameterBasedOnDbColumn("@isValid", isValid, VoteExamSurveyAnswerSchema.HZ_IsValid);
				command.AddParameterBasedOnDbColumn("@answerComment", answerComment, VoteExamSurveyAnswerSchema.HZ_AnswerComment);
				command.AddParameterBasedOnDbColumn("@questionOrder", questionOrder, VoteExamSurveyAnswerSchema.HZ_QuestionOrder);
				command.AddParameterBasedOnDbColumn("@subQuestionOrder", subQuestionOrder, VoteExamSurveyAnswerSchema.HZ_SubQuestionOrder);
				command.ExecuteNonQuery();
			}
			return pk;
		}

		#endregion

		#region CreateOrgBrandOrRelateName

		const string CreateOrgBrandOrRelatedNameSql = @"
INSERT INTO [dbo].[OrgBrandOrRelatedName] ([P1_PK], [P1_IsValid], [P1_RelatedName], [P1_OH], [P1_SystemCreateTimeUtc], [P1_SystemCreateUser], [P1_SystemLastEditTimeUtc], [P1_SystemLastEditUser])
VALUES (@pk, @isValid, @relatedName, @orgHeaderPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgBrandOrRelatedName(Guid orgHeaderPk, string relatedName, bool isValid)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgBrandOrRelatedNameSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@isValid", SqlDbType.Bit, isValid);
				command.AddParameter("@relatedName", SqlDbType.NVarChar, OrgBrandOrRelatedNameSchema.P1_RelatedName.MaxLength, relatedName);
				command.AddParameter("@orgHeaderPk", SqlDbType.UniqueIdentifier, orgHeaderPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateOrgPatternMatchOverride

		const string CreateOrgPatternMatchOverrideSql = @"
INSERT INTO [dbo].[OrgPatternMatchOverride] ([OO_PK], [OO_ForeignCode], [OO_LocalCode], [OO_OH], [OO_SystemCreateTimeUtc], [OO_SystemCreateUser], [OO_SystemLastEditTimeUtc], [OO_SystemLastEditUser])
VALUES (@pk, @foreignCode, @localCode, @orgHeaderPk, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateOrgPatternMatchOverride(Guid orgHeaderPk, string foreignCode, string localCode)
		{
			var pk = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateOrgPatternMatchOverrideSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@foreignCode", SqlDbType.VarChar, OrgPatternMatchOverrideSchema.OO_ForeignCode.MaxLength, foreignCode);
				command.AddParameter("@localCode", SqlDbType.VarChar, OrgPatternMatchOverrideSchema.OO_LocalCode.MaxLength, localCode);
				command.AddParameter("@orgHeaderPk", SqlDbType.UniqueIdentifier, orgHeaderPk);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateStmDialogDefault

		const string CreateStmDialogDefaultSql = @"
INSERT INTO [dbo].[StmDialogDefault] ([SDD_PK], [SDD_DialogIdentifier], [SDD_Owner], [SDD_SerializedDefaults], [SDD_Caption], [SDD_SystemCreateTimeUtc], [SDD_SystemCreateUser], [SDD_SystemLastEditTimeUtc], [SDD_SystemLastEditUser])
VALUES (@pk, @dialogIdentifier, @owner, @serializedDefaults, @caption, '~BP', GetUtcDate(), '~BP')";

		public Guid CreateStmDialogDefault(string serializedDefaults, string caption)
		{
			var pk = Guid.NewGuid();
			var dialogIdentifier = Guid.NewGuid();
			var owner = Guid.NewGuid();

			using (var command = Db.Connection.Command(CreateStmDialogDefaultSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@dialogIdentifier", SqlDbType.UniqueIdentifier, dialogIdentifier);
				command.AddParameter("@owner", SqlDbType.UniqueIdentifier, owner);
				command.AddParameter("@serializedDefaults", SqlDbType.VarChar, OrgPatternMatchOverrideSchema.OO_ForeignCode.MaxLength, serializedDefaults);
				command.AddParameter("@caption", SqlDbType.VarChar, OrgPatternMatchOverrideSchema.OO_LocalCode.MaxLength, caption);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobSupplierBooking

		const string CreateJobSupplierBookingSql = "INSERT INTO dbo.JobSupplierBooking([JSB_PK], [JSB_BookingId], [JSB_OH_BookingParty], [JSB_LoadMode], [JSB_TransportMode], [JSB_RL_NKLoadPort], [JSB_RL_NKDischargePort], [JSB_Status], [JSB_SystemCreateTimeUtc], [JSB_SystemCreateUser], [JSB_SystemLastEditTimeUtc],[JSB_SystemLastEditUser]) VALUES(@pk, @bookingId, @bookingParty, @loadMode, @transportMode, @loadPort, @dischargePort, @status, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobSupplierBooking(string bookingId, Guid bookingParty, string loadMode, string transportMode, string loadPort = "", string dischargePort = "", string status = "INC")
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateJobSupplierBookingSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@bookingId", SqlDbType.VarChar, JobSupplierBookingSchema.JSB_BookingId.MaxLength, bookingId);
				command.AddParameter("@bookingParty", SqlDbType.UniqueIdentifier, bookingParty);
				command.AddParameter("@loadMode", SqlDbType.VarChar, JobSupplierBookingSchema.JSB_LoadMode.MaxLength, loadMode);
				command.AddParameter("@transportMode", SqlDbType.VarChar, JobSupplierBookingSchema.JSB_LoadMode.MaxLength, transportMode);
				command.AddParameter("@loadPort", SqlDbType.VarChar, loadPort);
				command.AddParameter("@dischargePort", SqlDbType.VarChar, dischargePort);
				command.AddParameter("@status", SqlDbType.VarChar, status);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateJobSupplierBookingLine

		const string CreateJobSupplierBookingLineSql = "INSERT INTO dbo.JobSupplierBookingLine([JSL_PK], [JSL_JSB_Booking], [JSL_JO_OrderLine], [JSL_BookingLineId], [JSL_SystemCreateTimeUtc],  [JSL_SystemCreateUser],  [JSL_SystemLastEditTimeUtc], [JSL_SystemLastEditUser]) VALUES(@pk, @supplierBookingPK, @orderLinePK, @bookingLineId, GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

		public Guid CreateJobSupplierBookingLine(Guid supplierBookingPK, Guid orderLinePK, string bookingLineId)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateJobSupplierBookingLineSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@supplierBookingPK", SqlDbType.UniqueIdentifier, supplierBookingPK);
				command.AddParameter("@orderLinePK", SqlDbType.UniqueIdentifier, orderLinePK);
				command.AddParameter("@bookingLineId", SqlDbType.VarChar, bookingLineId);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateStmLink

		public Guid CreateStmLink(string moduleID, Guid logonCompany)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateStmLinkSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, StmLinkSchema.PK);
				command.AddParameterBasedOnDbColumn("@moduleID", moduleID, StmLinkSchema.STL_ModuleID);
				command.AddParameterBasedOnDbColumn("@logonCompany", logonCompany, StmLinkSchema.STL_GC_LogonCompany);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateCusEntryInstruction
		public Guid CreateCusEntryInstruction(
			Guid declarationPk,
			int clusterKey = 1,
			string addInfo = "",
			string style = "IFD",
			string subStyle = "D",
			string mergeBy = "NON",
			string description = "Auto-created instruction for IFD",
			string procedure = "",
			short displaySequence = 0,
			string dataModel = "DE")
		{
			var pk = Guid.NewGuid();
			using (DbCommand command = Db.Connection.Command(CreateCusEntryInstructionSql))
			{
				command.AddParameterBasedOnDbColumn("@CeiPK", pk, CusEntryInstructionSchema.PK);
				command.AddParameterBasedOnDbColumn("@CeiClusterKey", clusterKey, CusEntryInstructionSchema.CEI_ClusterKey);
				command.AddParameterBasedOnDbColumn("@DeclarationPK", declarationPk, CusEntryInstructionSchema.CEI_JE);
				command.AddParameterBasedOnDbColumn("@CeiAddInfo", addInfo, CusEntryInstructionSchema.CEI_AddInfo);
				command.AddParameterBasedOnDbColumn("@CeiStyle", style, CusEntryInstructionSchema.CEI_Style);
				command.AddParameterBasedOnDbColumn("@CeiSubstyle", subStyle, CusEntryInstructionSchema.CEI_SubStyle);
				command.AddParameterBasedOnDbColumn("@CeiMergeBy", mergeBy, CusEntryInstructionSchema.CEI_MergeBy);
				command.AddParameterBasedOnDbColumn("@CeiDescription", description, CusEntryInstructionSchema.CEI_Description);
				command.AddParameterBasedOnDbColumn("@CeiProcedure", procedure, CusEntryInstructionSchema.CEI_Procedure);
				command.AddParameterBasedOnDbColumn("@CeiDisplaySequence", displaySequence, CusEntryInstructionSchema.CEI_DisplaySequence);
				command.AddParameterBasedOnDbColumn("@CeiDataModel", dataModel, CusEntryInstructionSchema.CEI_DataModel);
				command.ExecuteNonQuery();
			}

			return pk;
		}
		#endregion CreateCusEntryInstruction

		#region CreateOrgTimetable

		public Guid CreateOrgTimetable(Guid orgAddressPk, string type, DateTime timeFrom, DateTime timeTo, bool monday = false,
			bool tuesday = false, bool wednesday = false, bool thursday = false, bool friday = false, bool saturday = false, bool sunday = false)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateOrgTimetableSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@orgAddressPk", SqlDbType.UniqueIdentifier, orgAddressPk);
				command.AddParameter("@type", SqlDbType.Char, type);
				command.AddParameter("@monday", SqlDbType.Bit, monday);
				command.AddParameter("@tuesday", SqlDbType.Bit, tuesday);
				command.AddParameter("@wednesday", SqlDbType.Bit, wednesday);
				command.AddParameter("@thursday", SqlDbType.Bit, thursday);
				command.AddParameter("@friday", SqlDbType.Bit, friday);
				command.AddParameter("@saturday", SqlDbType.Bit, saturday);
				command.AddParameter("@sunday", SqlDbType.Bit, sunday);
				command.AddParameter("@timeFrom", SqlDbType.SmallDateTime, timeFrom);
				command.AddParameter("@timeTo", SqlDbType.SmallDateTime, timeTo);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		public Guid CreateOrgTimetableWithEmptyAuditInfo(Guid orgAddressPk, string type, DateTime timeFrom, DateTime timeTo, bool monday = false,
			bool tuesday = false, bool wednesday = false, bool thursday = false, bool friday = false, bool saturday = false, bool sunday = false)
		{
			var pk = Guid.NewGuid();

			using (DbCommand command = Db.Connection.Command(CreateOrgTimetableWithEmptyAuditInfoSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@orgAddressPk", SqlDbType.UniqueIdentifier, orgAddressPk);
				command.AddParameter("@type", SqlDbType.Char, type);
				command.AddParameter("@monday", SqlDbType.Bit, monday);
				command.AddParameter("@tuesday", SqlDbType.Bit, tuesday);
				command.AddParameter("@wednesday", SqlDbType.Bit, wednesday);
				command.AddParameter("@thursday", SqlDbType.Bit, thursday);
				command.AddParameter("@friday", SqlDbType.Bit, friday);
				command.AddParameter("@saturday", SqlDbType.Bit, saturday);
				command.AddParameter("@sunday", SqlDbType.Bit, sunday);
				command.AddParameter("@timeFrom", SqlDbType.SmallDateTime, timeFrom);
				command.AddParameter("@timeTo", SqlDbType.SmallDateTime, timeTo);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion

		#region CreateRefLocoMap

		const string CreateRefLocoMapSql = @"
INSERT INTO [dbo].[RefLocoMap] 
	([RY_PK], [RY_LocalPortCode], [RY_RL_NKLocoPort], [RY_SystemUsage], [RY_RN], [RY_IsSystem], [RY_AutoVersion], [RY_SystemCreateTimeUtc], [RY_SystemCreateUser], [RY_SystemLastEditTimeUtc], [RY_SystemLastEditUser])
VALUES
	(@pk, @localPortCode, @locoPort, @systemUsage, @refCountryPk, 0, 0, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP' )";

		public Guid CreateRefLocoMap(string localPortCode, string locoPort, Guid refCountry)
		{
			var pk = Guid.NewGuid();
			const string systemUsage = "ALL";

			using var command = Db.Connection.Command(CreateRefLocoMapSql);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@localPortCode", SqlDbType.VarChar, RefLocoMapSchema.RY_LocalPortCode.MaxLength, localPortCode);
			command.AddParameter("@locoPort", SqlDbType.VarChar, RefLocoMapSchema.RY_RL_NKLocoPort.MaxLength, locoPort);
			command.AddParameter("@systemUsage", SqlDbType.VarChar, RefLocoMapSchema.RY_SystemUsage.MaxLength, systemUsage);
			command.AddParameter("@refCountryPk", SqlDbType.UniqueIdentifier, refCountry);
			command.ExecuteNonQuery();

			return pk;
		}

		#endregion

		#region CreateOrgSupplierPart

		const string CreateOrgSupplierPartSql = @"INSERT INTO [dbo].[OrgSupplierPart]([OP_PK], [OP_PartNum], [OP_SystemLastEditTimeUtc], [OP_SystemLastEditUser], [OP_SystemCreateTimeUtc], [OP_SystemCreateUser]) VALUES(@pk, @partNum, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')";

		public Guid CreateOrgSupplierPart(string partNum)
		{
			var pk = Guid.NewGuid();
			using var command = Db.Connection.Command(CreateOrgSupplierPartSql);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@partNum", SqlDbType.VarChar, OrgSupplierPartSchema.OP_PartNum.MaxLength, partNum);
			command.ExecuteNonQuery();
			return pk;
		}

		#endregion

		#region CreateCusClassPartPivot

		public Guid CreateCusClassPartPivot(Guid partPk, string countryCode, Guid? orgPk = null, string childType = "", string tariff = "", DateTime? creationTimeUtc = null, Guid? classification = null, string[] attribute1Values = null, string[] attribute2Values = null, string[] attribute3Values = null, string supplementalTariff = "")
		{
			var commandSql = @"INSERT INTO [dbo].[CusClassPartPivot]([CI_PK], [CI_OP], [CI_RN_NKCountry], [CI_OH], [CI_ChildType], [CI_TariffNum], [CI_SupplementalTariff], [CI_CC], [CI_SystemLastEditTimeUtc], [CI_SystemLastEditUser], [CI_SystemCreateTimeUtc], [CI_SystemCreateUser])
								VALUES(@ciPk, @partPK, @countryCode, @orgPk, @childType, @tariff, @supplementalTariff, @classificationPk, GETUTCDATE(), '~BP', @creationTimeUtc, '~BP')
								";

			var attributeInsertionSql = @"INSERT INTO[dbo].[CusAttributeFilter]([BG_PK], [BG_CI], [BG_AttributeName], [BG_AttributeValue1], [BG_SystemLastEditTimeUtc], [BG_SystemLastEditUser], [BG_SystemCreateTimeUtc], [BG_SystemCreateUser])
								VALUES";
			attribute1Values?.ForEach(value => attributeInsertionSql += $"{Environment.NewLine}(NewID(), @ciPk, 'AT1', '{value}', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),");
			attribute2Values?.ForEach(value => attributeInsertionSql += $"{Environment.NewLine}(NewID(), @ciPk, 'AT2', '{value}', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),");
			attribute3Values?.ForEach(value => attributeInsertionSql += $"{Environment.NewLine}(NewID(), @ciPk, 'AT3', '{value}', GETUTCDATE(), '~BP', GETUTCDATE(), '~BP'),");
			if ((attribute1Values?.Length ?? 0) + (attribute2Values?.Length ?? 0) + (attribute3Values?.Length ?? 0) > 0)
			{
				commandSql += attributeInsertionSql.Substring(0, attributeInsertionSql.Length - 1);
			}
			var ciPk = Guid.NewGuid();

			using var command = Db.Connection.Command(commandSql);
			command.AddParameter("@ciPk", SqlDbType.UniqueIdentifier, ciPk);
			command.AddParameter("@partPK", SqlDbType.UniqueIdentifier, partPk);
			command.AddParameter("@countryCode", SqlDbType.VarChar, CusClassPartPivotSchema.CI_RN_NKCountry.MaxLength, countryCode);
			command.AddParameter("@orgPK", SqlDbType.UniqueIdentifier, orgPk == null ? DBNull.Value : orgPk);
			command.AddParameter("@childType", SqlDbType.VarChar, childType);
			command.AddParameter("@tariff", SqlDbType.VarChar, tariff);
			command.AddParameter("@supplementalTariff", SqlDbType.VarChar, supplementalTariff);
			command.AddParameter("@classificationPk", SqlDbType.UniqueIdentifier, classification == null ? DBNull.Value : classification);
			command.AddParameter("@creationTimeUtc", SqlDbType.SmallDateTime, creationTimeUtc == null ? DateTime.UtcNow : creationTimeUtc);
			command.ExecuteNonQuery();
			return ciPk;
		}

		#endregion

		#region CreateCusClassification

		const string CreateCusClassificationSql = @"INSERT INTO [dbo].[CusClassification]([CC_PK], [CC_ClassificationType], [CC_LookupCode], [CC_TariffNum], [CC_RN_NKCountryCode], [CC_Description], [CC_SystemLastEditTimeUtc], [CC_SystemLastEditUser], [CC_SystemCreateTimeUtc], [CC_SystemCreateUser]) VALUES(@pk, @classificationType, @lookupCode ,@tariffNum, @countryCode, @description, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')";

		public Guid CreateCusClassification(string classificationType, string lookupCode, string tariffNum, string countryCode, string desc)
		{
			var pk = Guid.NewGuid();
			using var command = Db.Connection.Command(CreateCusClassificationSql);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@classificationType", SqlDbType.Char, classificationType);
			command.AddParameter("@lookupCode", SqlDbType.Char, lookupCode);
			command.AddParameter("@tariffNum", SqlDbType.VarChar, CusClassificationSchema.CC_TariffNum.MaxLength, tariffNum);
			command.AddParameter("@countryCode", SqlDbType.Char, countryCode);
			command.AddParameter("@description", SqlDbType.VarChar, CusClassificationSchema.CC_Description.MaxLength, desc);
			command.ExecuteNonQuery();
			return pk;
		}

		#endregion

		#region CreateCusUSClassification

		const string CreateCusUSClassificationSql = @"INSERT INTO [dbo].[CusUSClassification]([CD_PK], [CD_TaxRate], [CD_TaxRateDesc], [CD_ParentID], [CD_ParentTableCode], [CD_SystemLastEditTimeUtc], [CD_SystemLastEditUser], [CD_SystemCreateTimeUtc], [CD_SystemCreateUser]) VALUES(@pk, @taxRate, @taxRateDesc, @parentPK, @parentTableCode, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')";

		public Guid CreateCusUSClassification(Guid parent, decimal taxRate = decimal.Zero, string taxRateDesc = "", string parentTableCode = "CI")
		{
			var pk = Guid.NewGuid();
			using var command = Db.Connection.Command(CreateCusUSClassificationSql);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parent);
			command.AddParameter("@parentTableCode", SqlDbType.VarChar, CusUSClassificationSchema.CD_ParentTableCode.MaxLength, parentTableCode);
			command.AddParameter("@taxRate", SqlDbType.Decimal, taxRate);
			command.AddParameter("@taxRateDesc", SqlDbType.VarChar, CusUSClassificationSchema.CD_TaxRateDesc.MaxLength, taxRateDesc);
			command.ExecuteNonQuery();
			return pk;
		}

		#endregion

		#region Create LandedCostHeader

		const string CreateLandedCostHeaderSql = @"
			INSERT INTO [dbo].[LandedCostHeader]
					(
						[LT_PK],
						[LT_IsValid],
						[LT_LandedCostType],
						[LT_EstimatedLandedCostComment],
						[LT_DefaultEstimatedDutyRate],
						[LT_DateOfEntry],
						[LT_DateOfProcessing],
						[LT_ParentID],
						[LT_ParentTableCode],
						[LT_GC],
						[LT_SystemCreateTimeUtc],
						[LT_SystemCreateUser],
						[LT_SystemLastEditTimeUtc],
						[LT_SystemLastEditUser]
					)
			VALUES
					( @LT_PK,
					  1,
					  @LT_LandedCostType,
					  '',
					  @LT_DefaultEstimatedDutyRate,
					  @LT_DateOfEntry,
					  @LT_DateOfProcessing,
					  @LT_ParentID,
					  @LT_ParentTableCode,
					  @LT_GC,
					  GETUTCDATE(),
					  '~BP',
					  GETUTCDATE(),
					  '~BP' )";

		public Guid CreateLandedCostHeader(string landedCostType,
			decimal defaultEstimatedDutyRate,
			DateTime dateOfEntry,
			Guid parentId,
			string parentTableCode,
			Guid companyPk)
		{
			var pk = Guid.NewGuid();

			using var command = Db.Connection.Command(CreateLandedCostHeaderSql);
			command.AddParameter("@LT_PK", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@LT_LandedCostType", SqlDbType.VarChar, LandedCostHeaderSchema.LT_LandedCostType.MaxLength, landedCostType);
			command.AddParameter("@LT_DefaultEstimatedDutyRate", SqlDbType.Decimal, defaultEstimatedDutyRate);
			command.AddParameter("@LT_DateOfEntry", SqlDbType.DateTime, dateOfEntry);
			command.AddParameter("@LT_DateOfProcessing", SqlDbType.DateTime, dateOfEntry);
			command.AddParameter("@LT_ParentID", SqlDbType.UniqueIdentifier, parentId);
			command.AddParameter("@LT_ParentTableCode", SqlDbType.VarChar, LandedCostHeaderSchema.LT_ParentTableCode.MaxLength, parentTableCode);
			command.AddParameter("@LT_GC", SqlDbType.UniqueIdentifier, companyPk);

			command.ExecuteNonQuery();

			return pk;
		}

		#endregion

		#region Create LandCostInput

		const string CreateLandCostInputSql = @"
			INSERT INTO [dbo].[LandCostInput]
					(
						[LI_PK],
						[LI_IsValid],
						[LI_AC_ChargeCode],
						[LI_ChargeDescription],
						[LI_LandedCostGroup],
						[LI_DistributeCostBy],
						[LI_CostAmount],
						[LI_RX_NKCostCurrency],
						[LI_ServiceExRate],
						[LI_IsParentGroupInvoice],
						[LI_IsUserEntered],
						[LI_ParentID],
						[LI_ParentTableCode],
						[LI_LT],
						[LI_SystemCreateTimeUtc],
						[LI_SystemCreateUser],
						[LI_SystemLastEditTimeUtc],
						[LI_SystemLastEditUser]
					)
			VALUES
					( @LI_PK,
					  1,
					  NULL,
					  @LI_ChargeDescription,
					  1,
					  'VOL',
					  @LI_CostAmount,
					  'USD',
					  1,
					  1,
					  1,
					  @LI_ParentID,
					  @LI_ParentTableCode,
					  @LI_LT,
					  GETUTCDATE(),
					  '~BP',
					  GETUTCDATE(),
					  '~BP')";

		public Guid CreateLandCostInput(string chargeDesc,
			decimal costAmount,
			Guid parentId,
			string parentTableCode,
			Guid lCHeaderPk)
		{
			var pk = Guid.NewGuid();

			using var command = Db.Connection.Command(CreateLandCostInputSql);
			command.AddParameter("@LI_PK", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@LI_ChargeDescription", SqlDbType.VarChar, LandCostInputSchema.LI_ChargeDescription.MaxLength, chargeDesc);
			command.AddParameter("@LI_CostAmount", SqlDbType.Decimal, costAmount);
			command.AddParameter("@LI_ParentID", SqlDbType.UniqueIdentifier, parentId);
			command.AddParameter("@LI_ParentTableCode", SqlDbType.VarChar, LandedCostHeaderSchema.LT_ParentTableCode.MaxLength, parentTableCode);
			command.AddParameter("@LI_LT", SqlDbType.UniqueIdentifier, lCHeaderPk);

			command.ExecuteNonQuery();

			return pk;
		}

		#endregion

		#region Create LandedCostHistory

		const string CreateLandedCostHistorySql = @"
			INSERT INTO [dbo].[LandedCostHistory]
					(
						[LH_PK],
						[LH_IsValid],
						[LH_LandedCostHistoryLineType],
						[LH_RN_NKCountryOfEntry],
						[LH_DutyPercent],
						[LH_LandedCostMarginPercent1],
						[LH_LandedCostMarginPercent2],
						[LH_LandedCostMarginPercent3],
						[LH_LT],
						[LH_OP],
						[LH_ParentTableCode],
						[LH_ParentID],
						[LH_SystemCreateTimeUtc],
						[LH_SystemCreateUser],
						[LH_SystemLastEditTimeUtc],
						[LH_SystemLastEditUser]
					)
			VALUES
					( @LH_PK,
					  1,
					  @LH_LandedCostHistoryLineType,
					  @LH_RN_NKCountryOfEntry,
					  @LH_DutyPercent,
					  0.0,
					  0.0,
					  0.0,
					  @LH_LT,
					  NULL,
					  @LH_ParentTableCode,
					  NEWID(),
					  GETUTCDATE(),
					  '~BP',
					  GETUTCDATE(),
					  '~BP')";

		public Guid CreateLandedCostHistory(string landedCostHistoryLineType,
			string countryOfEntry,
			decimal dutyPercent,
			Guid lCHeaderPk,
			string parentTableCode)
		{
			var pk = Guid.NewGuid();

			using var command = Db.Connection.Command(CreateLandedCostHistorySql);
			command.AddParameter("@LH_PK", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@LH_LandedCostHistoryLineType", SqlDbType.VarChar, LandedCostHistorySchema.LH_LandedCostHistoryLineType.MaxLength, landedCostHistoryLineType);
			command.AddParameter("@LH_RN_NKCountryOfEntry", SqlDbType.VarChar, LandedCostHistorySchema.LH_RN_NKCountryOfEntry.MaxLength, countryOfEntry);
			command.AddParameter("@LH_DutyPercent", SqlDbType.Decimal, dutyPercent);
			command.AddParameter("@LH_LT", SqlDbType.UniqueIdentifier, lCHeaderPk);
			command.AddParameter("@LH_ParentTableCode", SqlDbType.VarChar, LandedCostHistorySchema.LH_ParentTableCode.MaxLength, parentTableCode);

			command.ExecuteNonQuery();

			return pk;
		}

		#endregion

		#region Create LandedLineCostItem

		const string CreateLandedLineCostItemSql = @"
			INSERT INTO [dbo].[LandedLineCostItem]
					(
						[LZ_PK],
						[LZ_IsValid],
						[LZ_CostType],
						[LZ_CostAmount],
						[LZ_LH],
						[LZ_SystemCreateTimeUtc],
						[LZ_SystemCreateUser],
						[LZ_SystemLastEditTimeUtc],
						[LZ_SystemLastEditUser]
					)
			VALUES
					( @LZ_PK,
					  1,
					  @LZ_CostType,
					  @LZ_CostAmount,
					  @LZ_LH,
					  GETUTCDATE(),
					  '~BP',
					  GETUTCDATE(),
					  '~BP')";

		public Guid CreateLandedLineCostItem(string costType,
			decimal costAmount,
			Guid lCHistoryPk)
		{
			var pk = Guid.NewGuid();

			using var command = Db.Connection.Command(CreateLandedLineCostItemSql);
			command.AddParameter("@LZ_PK", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@LZ_CostType", SqlDbType.VarChar, LandedLineCostItemSchema.LZ_CostType.MaxLength, costType);
			command.AddParameter("@LZ_CostAmount", SqlDbType.Decimal, costAmount);
			command.AddParameter("@LZ_LH", SqlDbType.UniqueIdentifier, lCHistoryPk);

			command.ExecuteNonQuery();

			return pk;
		}

		#endregion

		#region Create CusVehicle

		const string CreateCusVehicleSql =
			"INSERT INTO dbo.CusVehicle(CVH_PK, CVH_ParentID, CVH_ParentTableCode, CVH_ClusterKey, CVH_DataModel, CVH_VehicleIdentificationNumber, CVH_SystemCreateTimeUtc, CVH_SystemCreateUser, CVH_SystemLastEditTimeUtc, CVH_SystemLastEditUser) VALUES (@PK, @ParentPK, @ParentTableCode, @ClusterKey, @CountryCode, 'FirstInserted', GetUtcDate(), '~BP', GetUtcDate(), '~BP');";

		public Guid CreateCusVehicle(Guid parentPk, string parentTableCode, string countryCode, int clusterKey = 0)
		{
			var pk = Guid.NewGuid();

			using var command = Db.Connection.Command(CreateCusVehicleSql);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@parentPk", SqlDbType.UniqueIdentifier, parentPk);
			command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
			command.AddParameter("@countryCode", SqlDbType.VarChar, countryCode);
			command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
			command.ExecuteNonQuery();

			return pk;
		}

		#endregion

		#region Create CusEngine

		const string CreateCusEngineSql =
			"INSERT INTO dbo.CusEngine(CEG_PK, CEG_ParentID, CEG_ParentTableCode, CEG_ClusterKey, CEG_DataModel, CEG_SystemCreateTimeUtc, CEG_SystemCreateUser, CEG_SystemLastEditTimeUtc, CEG_SystemLastEditUser) VALUES (@PK, @ParentPK, @ParentTableCode, @ClusterKey, @CountryCode, GetUtcDate(), '~BP', GetUtcDate(), '~BP');";

		public Guid CreateCusEngine(Guid parentPk, string parentTableCode, string countryCode, int clusterKey = 0)
		{
			var pk = Guid.NewGuid();

			using var command = Db.Connection.Command(CreateCusEngineSql);
			command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
			command.AddParameter("@parentPk", SqlDbType.UniqueIdentifier, parentPk);
			command.AddParameter("@parentTableCode", SqlDbType.VarChar, parentTableCode);
			command.AddParameter("@countryCode", SqlDbType.VarChar, countryCode);
			command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);
			command.ExecuteNonQuery();

			return pk;
		}

		#endregion

		#region Update Records

		public void UpdateTableColumn(string tableName,
			string columnName,
			object columnValue,
			SqlDbType columnType,
			string primaryKeyColumnName,
			object primaryKeyValue,
			DateTime updateTime = default,
			string updateUser = null)
		{
			var tablePrefix = primaryKeyColumnName.Split(new[] { '_' })[0];
			var updateSetColumns = $"{columnName} = @ColumnValue, {tablePrefix}_SystemLastEditTimeUtc = @updateTime, {tablePrefix}_SystemLastEditUser = @updateUser";

			using (var command = Db.Connection.Command($"UPDATE {tableName} SET {updateSetColumns} WHERE {primaryKeyColumnName} = @PrimaryKeyValue"))
			{
				command.AddParameter("@ColumnValue", columnType, columnValue);
				command.AddParameter("@PrimaryKeyValue", SqlDbType.UniqueIdentifier, primaryKeyValue);
				command.AddParameter("@updateTime", SqlDbType.SmallDateTime, updateTime == default ? DateTime.UtcNow : updateTime);
				command.AddParameter("@updateUser", SqlDbType.VarChar, updateUser ?? "~SF");
				command.ExecuteNonQuery();
			}
		}

		public void UpdateDeclarationColumn(string columnName,
			object columnValue,
			SqlDbType columnType,
			object primaryKeyValue,
			DateTime updateTime = default,
			string updateUser = null)
		{
			UpdateTableColumn("JobDeclaration",
				columnName,
				columnValue,
				columnType,
				JobDeclarationSchema.PK.Name,
				primaryKeyValue,
				updateTime,
				updateUser);
		}

		#endregion

		#region CreateStmData

		public Guid CreateStmData()
		{
			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command(CreateStmDataSql))
			{
				command.AddParameterBasedOnDbColumn("@pk", pk, StmDataSchema.PK);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion
	}
}
