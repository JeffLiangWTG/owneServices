using System;
using System.Text;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.TransitWarehouse.Testing
{
	[TestedType(typeof(UpdatePackageStateSecurityStatusConsideringTransitJobDirection))]
	public class UpdatePackageStateSecurityStatusConsideringTransitJobDirectionTest : DataTransformationTestCase
	{
		#region Tests_ImportAndDomesticDirection

		#region Tests_ImportAndDomesticDirection_TopHU_MidHU_2_Inners

		public void Test_P1_REQ_P2_REQ_RCN_IMP_DCN_IMP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_NOT_P2_NOT_RCN_IMP_DCN_IMP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: "NOT", topOuterStat: "NOT", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_OVR_P2_OVR_RCN_IMP_DCN_IMP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: "OVR", topOuterStat: "OVR", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", expectedMidOuterStat: "OVR", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_REQ_P2_NOT_RCN_IMP_DCN_IMP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_IMP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_NOT_P2_OVR_RCN_IMP_DCN_IMP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: "NOT", topOuterStat: "NOT", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_REQ_P2_REQ_RCN_DOM_DCN_DOM_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_NOT_P2_NOT_RCN_DOM_DCN_DOM_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: "NOT", topOuterStat: "NOT", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_OVR_P2_OVR_RCN_DOM_DCN_DOM_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: "OVR", topOuterStat: "OVR", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", expectedMidOuterStat: "OVR", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_REQ_P2_NOT_RCN_DOM_DCN_DOM_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_DOM_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_NOT_P2_OVR_RCN_DOM_DCN_DOM_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: "NOT", topOuterStat: "NOT", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_SEC_P2_HRS_RCN_DOM_DCN_IMP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "DOM", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_HRS_P2_OVR_RCN_IMP_DCN_DOM_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_SEC_P2_SCR_RCN_IMP_DCN_DOM_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: "SEC", topOuterStat: "SEC", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");

		#endregion

		#region Tests_ImportAndDomesticDirection_TopOVP_MidOVP_2_Inners

		public void Test_P1_REQ_P2_REQ_RCN_IMP_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_NOT_P2_NOT_RCN_IMP_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: "SCR", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_OVR_P2_OVR_RCN_IMP_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: "OVR", topOuterStat: "SEC", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "OVR", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_NOT_RCN_IMP_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "HRS", topOuterStat: "SCR", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "HRN", topOuterStat: "OVR", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_NOT_P2_OVR_RCN_IMP_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "NOT", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_REQ_RCN_DOM_DCN_DOM_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "NOT", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_NOT_P2_NOT_RCN_DOM_DCN_DOM_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "HRN", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_OVR_P2_OVR_RCN_DOM_DCN_DOM_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_NOT_RCN_DOM_DCN_DOM_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_DOM_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_NOT_P2_OVR_RCN_DOM_DCN_DOM_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: "NOT", topOuterStat: "HRS", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_SEC_P2_HRS_RCN_DOM_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: "SEC", topOuterStat: "SCR", rcnDirection: "DOM", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_HRS_P2_OVR_RCN_IMP_DCN_DOM_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "SEC", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_SEC_P2_SCR_RCN_IMP_DCN_DOM_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: "SEC", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");

		#endregion

		#region Tests_ImportAndDomesticDirection_TopHU_MidOVP_2_Inners

		public void Test_P1_REQ_P2_REQ_RCN_IMP_DCN_IMP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_NOT_P2_NOT_RCN_IMP_DCN_IMP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: "SCR", topOuterStat: "NOT", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_OVR_P2_OVR_RCN_IMP_DCN_IMP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: "OVR", topOuterStat: "OVR", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", expectedMidOuterStat: "OVR", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_NOT_RCN_IMP_DCN_IMP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "HRS", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_IMP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "HRN", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_NOT_P2_OVR_RCN_IMP_DCN_IMP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "NOT", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_REQ_RCN_DOM_DCN_DOM_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "NOT", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_NOT_P2_NOT_RCN_DOM_DCN_DOM_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "NOT", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_OVR_P2_OVR_RCN_DOM_DCN_DOM_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "OVR", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_NOT_RCN_DOM_DCN_DOM_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_DOM_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_NOT_P2_OVR_RCN_DOM_DCN_DOM_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: "NOT", topOuterStat: "NOT", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_SEC_P2_HRS_RCN_DOM_DCN_IMP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: "SEC", topOuterStat: "HRS", rcnDirection: "DOM", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_HRS_P2_OVR_RCN_IMP_DCN_DOM_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");
		public void Test_P1_SEC_P2_SCR_RCN_IMP_DCN_DOM_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: "SEC", topOuterStat: "SEC", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP");

		#endregion

		#region Tests_ImportAndDomesticDirection_TopHU_2_Inners

		public void Test_P1_REQ_P2_REQ_RCN_IMP_DCN_IMP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_NOT_RCN_IMP_DCN_IMP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_OVR_P2_OVR_RCN_IMP_DCN_IMP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: null, topOuterStat: "OVR", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_NOT_RCN_IMP_DCN_IMP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_IMP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_OVR_RCN_IMP_DCN_IMP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_REQ_RCN_DOM_DCN_DOM_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_NOT_RCN_DOM_DCN_DOM_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_OVR_P2_OVR_RCN_DOM_DCN_DOM_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: null, topOuterStat: "OVR", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_NOT_RCN_DOM_DCN_DOM_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_DOM_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_OVR_RCN_DOM_DCN_DOM_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_HRS_RCN_DOM_DCN_IMP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "DOM", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_OVR_RCN_IMP_DCN_DOM_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_SCR_RCN_IMP_DCN_DOM_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);

		#endregion

		#region Tests_ImportAndDomesticDirection_TopOVP_2_Inners

		public void Test_P1_REQ_P2_REQ_RCN_IMP_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_NOT_RCN_IMP_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_OVR_P2_OVR_RCN_IMP_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: null, topOuterStat: "OVR", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_NOT_RCN_IMP_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_OVR_RCN_IMP_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_REQ_RCN_DOM_DCN_DOM_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_NOT_RCN_DOM_DCN_DOM_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_OVR_P2_OVR_RCN_DOM_DCN_DOM_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: null, topOuterStat: "OVR", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_NOT_RCN_DOM_DCN_DOM_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_DOM_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_OVR_RCN_DOM_DCN_DOM_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "DOM", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_HRS_RCN_DOM_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "DOM", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_OVR_RCN_IMP_DCN_DOM_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_SCR_RCN_IMP_DCN_DOM_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);

		#endregion

		#endregion

		#region Tests_ExportAndUnknownDirection

		#region Tests_ExportAndUnknownDirection_TopHU_MidHU_2_Inners

		public void Test_P1_REQ_P2_REQ_RCN_EXP_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "REQ", expectedMidOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_NOT_P2_NOT_RCN_EXP_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: "NOT", topOuterStat: "NOT", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_OVR_P2_OVR_RCN_EXP_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: "OVR", topOuterStat: "OVR", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", expectedMidOuterStat: "OVR", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_REQ_P2_NOT_RCN_EXP_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", expectedMidOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", expectedMidOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_NOT_P2_OVR_RCN_EXP_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "HRS", expectedMidOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_HRS_P2_OVR_RCN_EXP_DCN_Unknown_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", expectedMidOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_SEC_P2_SCR_RCN_Unknown_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: "SEC", topOuterStat: "SEC", rcnDirection: "", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "SEC", expectedMidOuterStat: "SEC", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_REQ_P2_REQ_RCN_Unknown_DCN_Unknown_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "REQ", expectedMidOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_NOT_P2_NOT_RCN_Unknown_DCN_Unknown_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: "NOT", topOuterStat: "NOT", rcnDirection: "", dcnDirection: "", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_OVR_P2_OVR_RCN_Unknown_DCN_Unknown_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: "OVR", topOuterStat: "OVR", rcnDirection: "", dcnDirection: "", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", expectedMidOuterStat: "OVR", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_REQ_P2_NOT_RCN_Unknown_DCN_Unknown_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", expectedMidOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_Unknown_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", expectedMidOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_NOT_P2_OVR_RCN_Unknown_DCN_Unknown_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: "NOT", topOuterStat: "NOT", rcnDirection: "", dcnDirection: "", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_SEC_P2_HRS_RCN_Unknown_DCN_Unknown_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "", dcnDirection: "", expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "HRS", expectedMidOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_HRS_P2_OVR_RCN_Unknown_DCN_Unknown_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "", dcnDirection: "", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", expectedMidOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "HU");
		public void Test_P1_SEC_P2_SCR_RCN_Unknown_DCN_Unknown_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: "SEC", topOuterStat: "SEC", rcnDirection: "", dcnDirection: "", expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "SEC", expectedMidOuterStat: "SEC", topOuterUnitType: "HU", midOuterUnitType: "HU");

		#endregion

		#region Tests_ExportAndUnknownDirection_TopOVP_MidOVP_2_Inners

		public void Test_P1_REQ_P2_REQ_RCN_EXP_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "HRS", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "REQ", expectedMidOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_NOT_P2_NOT_RCN_EXP_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: "NOT", topOuterStat: "OVR", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "OVR", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_OVR_P2_OVR_RCN_EXP_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: "OVR", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", expectedMidOuterStat: "OVR", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_NOT_RCN_EXP_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "NOT", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", expectedMidOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "SEC", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", expectedMidOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_NOT_P2_OVR_RCN_EXP_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: "REQ", topOuterStat: "SCR", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "SCR", expectedMidOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_HRS_P2_OVR_RCN_EXP_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "SEC", topOuterStat: "HRN", rcnDirection: "EXP", dcnDirection: "", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRN", expectedMidOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_SEC_P2_SCR_RCN_Unknown_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: "HRN", topOuterStat: "HRS", rcnDirection: "", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "HRS", expectedMidOuterStat: "HRN", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_REQ_RCN_Unknown_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "SEC", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "SEC", expectedMidOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_NOT_P2_NOT_RCN_Unknown_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: "NOT", topOuterStat: "SEC", rcnDirection: "", dcnDirection: "", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "SEC", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_OVR_P2_OVR_RCN_Unknown_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: "OVR", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", expectedMidOuterStat: "OVR", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_NOT_RCN_Unknown_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", expectedMidOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "SCR", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", expectedMidOuterStat: "SCR", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_NOT_P2_OVR_RCN_Unknown_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: "NOT", topOuterStat: "SEC", rcnDirection: "", dcnDirection: "", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", expectedMidOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_SEC_P2_HRS_RCN_Unknown_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: "REQ", topOuterStat: "HRS", rcnDirection: "", dcnDirection: "", expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "HRS", expectedMidOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_HRS_P2_OVR_RCN_Unknown_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "NOT", rcnDirection: "", dcnDirection: "", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", expectedMidOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: "OVP");
		public void Test_P1_SEC_P2_SCR_RCN_Unknown_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: "SEC", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "REQ", expectedMidOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: "OVP");

		#endregion

		#region Tests_ExportAndUnknownDirection_TopHU_MidOVP_2_Inners

		public void Test_P1_REQ_P2_REQ_RCN_EXP_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "HRS", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "HRS");
		public void Test_P1_NOT_P2_NOT_RCN_EXP_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: "NOT", topOuterStat: "NOT", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_OVR_P2_OVR_RCN_EXP_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: "OVR", topOuterStat: "OVR", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "OVR");
		public void Test_P1_REQ_P2_NOT_RCN_EXP_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_NOT_P2_OVR_RCN_EXP_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: "REQ", topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_HRS_P2_OVR_RCN_EXP_DCN_Unknown_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "SEC", topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "SEC");
		public void Test_P1_SEC_P2_SCR_RCN_Unknown_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: "HRN", topOuterStat: "SEC", rcnDirection: "", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "SEC", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "HRN");
		public void Test_P1_REQ_P2_REQ_RCN_Unknown_DCN_Unknown_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_NOT_P2_NOT_RCN_Unknown_DCN_Unknown_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: "NOT", topOuterStat: "NOT", rcnDirection: "", dcnDirection: "", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_OVR_P2_OVR_RCN_Unknown_DCN_Unknown_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: "OVR", topOuterStat: "OVR", rcnDirection: "", dcnDirection: "", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "OVR");
		public void Test_P1_REQ_P2_NOT_RCN_Unknown_DCN_Unknown_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_Unknown_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "SCR", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "SCR");
		public void Test_P1_NOT_P2_OVR_RCN_Unknown_DCN_Unknown_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: "NOT", topOuterStat: "NOT", rcnDirection: "", dcnDirection: "", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_SEC_P2_HRS_RCN_Unknown_DCN_Unknown_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: "REQ", topOuterStat: "HRS", rcnDirection: "", dcnDirection: "", expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_HRS_P2_OVR_RCN_Unknown_DCN_Unknown_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "", dcnDirection: "", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "HRS");
		public void Test_P1_SEC_P2_SCR_RCN_Unknown_DCN_Unknown_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: "SEC", topOuterStat: "SEC", rcnDirection: "", dcnDirection: "", expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "SEC", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "SEC");

		#endregion

		#region Tests_ExportAndUnknownDirection_TopHU_2_Inners

		public void Test_P1_REQ_P2_REQ_RCN_EXP_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_NOT_RCN_EXP_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_OVR_P2_OVR_RCN_EXP_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: null, topOuterStat: "OVR", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_NOT_RCN_EXP_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_OVR_RCN_EXP_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_OVR_RCN_EXP_DCN_Unknown_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_SCR_RCN_Unknown_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "SEC", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_REQ_RCN_Unknown_DCN_Unknown_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_NOT_RCN_Unknown_DCN_Unknown_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "", dcnDirection: "", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_OVR_P2_OVR_RCN_Unknown_DCN_Unknown_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: null, topOuterStat: "OVR", rcnDirection: "", dcnDirection: "", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_NOT_RCN_Unknown_DCN_Unknown_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_Unknown_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_OVR_RCN_Unknown_DCN_Unknown_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "", dcnDirection: "", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_HRS_RCN_Unknown_DCN_Unknown_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "", dcnDirection: "", expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_OVR_RCN_Unknown_DCN_Unknown_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "", dcnDirection: "", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_SCR_RCN_Unknown_DCN_Unknown_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "", dcnDirection: "", expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "SEC", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);

		#endregion

		#region Tests_ExportAndUnknownDirection_TopOVP_2_Inners

		public void Test_P1_REQ_P2_REQ_RCN_EXP_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_NOT_RCN_EXP_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_OVR_P2_OVR_RCN_EXP_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: null, topOuterStat: "OVR", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_NOT_RCN_EXP_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_OVR_RCN_EXP_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_OVR_RCN_EXP_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_SCR_RCN_Unknown_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_REQ_RCN_Unknown_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_NOT_RCN_Unknown_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "", dcnDirection: "", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_OVR_P2_OVR_RCN_Unknown_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "OVR", p2Stat: "OVR", midOuterStat: null, topOuterStat: "OVR", rcnDirection: "", dcnDirection: "", expectedP1Stat: "OVR", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_NOT_RCN_Unknown_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_OVR_RCN_Unknown_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "NOT", p2Stat: "OVR", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "", dcnDirection: "", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_HRS_RCN_Unknown_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "", dcnDirection: "", expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_OVR_RCN_Unknown_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "", dcnDirection: "", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_SCR_RCN_Unknown_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "", dcnDirection: "", expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);

		#endregion

		#endregion

		#region Tests_MixedDirection

		#region Tests_MixedDirection_TopHU_MidHU_2_Inners

		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_IMP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_DOM_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_DOM_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "NOT");
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_IMP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "NOT");
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_Unknown_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_IMP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "IMP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_DOM_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_Unknown_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_Unknown_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_SEC_P2_OVR_RCN_IMP_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "OVR", midOuterStat: "SEC", topOuterStat: "SEC", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "SEC");
		public void Test_P1_SEC_P2_OVR_RCN_EXP_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "OVR", midOuterStat: "SEC", topOuterStat: "SEC", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "SEC");
		public void Test_P1_HRS_P2_OVR_RCN_IMP_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "HRS");
		public void Test_P1_HRS_P2_OVR_RCN_EXP_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "HRS");
		public void Test_P1_HRS_P2_NOT_RCN_EXP_DCN_IMP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "NOT", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "HRS", expectedP2Stat: "NOT", expectedTopOuterStat: "HRS", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "HRS");
		public void Test_P1_HRS_P2_SEC_RCN_EXP_DCN_IMP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "HRS", expectedP2Stat: "SEC", expectedTopOuterStat: "HRS", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "HRS");
		public void Test_P1_HRS_P2_HRN_RCN_IMP_DCN_IMP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "NOT");
		public void Test_P1_HRS_P2_HRN_RCN_EXP_DCN_EXP_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: "HRS", topOuterStat: "HRS ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "HRN", expectedTopOuterStat: "HRS", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "HRS");

		#endregion

		#region Tests_MixedDirection_TopOVP_MidOVP_2_Inners

		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "HRN", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "HRN", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "HRS");
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "NOT", topOuterStat: "HRN", rcnDirection: "DOM", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "HRN", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_DOM_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "HRN", topOuterStat: "SEC", rcnDirection: "EXP", dcnDirection: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "HRN");
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_DOM_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "NOT", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "SEC", topOuterStat: "SEC", rcnDirection: "DOM", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "OVR", topOuterStat: "SCR", rcnDirection: "IMP", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "SCR", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "OVR");
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "SCR", topOuterStat: "HRS", rcnDirection: "", dcnDirection: "IMP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "SCR");
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_DOM_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "SEC", rcnDirection: "", dcnDirection: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_SEC_P2_OVR_RCN_IMP_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "OVR", midOuterStat: "SEC", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "SEC");
		public void Test_P1_SEC_P2_OVR_RCN_EXP_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "OVR", midOuterStat: "SEC", topOuterStat: "HRN", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "OVR", expectedTopOuterStat: "HRN", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "SEC");
		public void Test_P1_HRS_P2_OVR_RCN_IMP_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "SEC", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "HRS");
		public void Test_P1_HRS_P2_OVR_RCN_EXP_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "HRN", topOuterStat: "SCR", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "SCR", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "HRN");
		public void Test_P1_HRS_P2_NOT_RCN_EXP_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "HRS", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_HRS_P2_SEC_RCN_EXP_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: "OVR", topOuterStat: "HRN", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "HRS", expectedP2Stat: "SEC", expectedTopOuterStat: "HRN", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "OVR");
		public void Test_P1_HRS_P2_HRN_RCN_IMP_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: "REQ", topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_HRS_P2_HRN_RCN_EXP_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "HRN", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");

		#endregion

		#region Tests_MixedDirection_TopHU_MidOVP_2_Inners

		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_IMP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "HRS");
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "NOT", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_DOM_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "HRN", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "HRN");
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_DOM_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_IMP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "SEC", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_Unknown_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "OVR", topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "OVR");
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_IMP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "SCR", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "IMP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "SCR");
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_DOM_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_Unknown_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_Unknown_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_SEC_P2_OVR_RCN_IMP_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "OVR", midOuterStat: "SEC", topOuterStat: "SEC", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "SEC");
		public void Test_P1_SEC_P2_OVR_RCN_EXP_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "OVR", midOuterStat: "SEC", topOuterStat: "SEC", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "SEC");
		public void Test_P1_HRS_P2_OVR_RCN_IMP_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "HRS");
		public void Test_P1_HRS_P2_OVR_RCN_EXP_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "HRN", topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "HRN");
		public void Test_P1_HRS_P2_NOT_RCN_EXP_DCN_IMP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "HRS", expectedP2Stat: "NOT", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_HRS_P2_SEC_RCN_EXP_DCN_IMP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: "OVR", topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "HRS", expectedP2Stat: "SEC", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "OVR");
		public void Test_P1_SEC_P2_HRN_RCN_IMP_DCN_IMP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRN", midOuterStat: "REQ", topOuterStat: "HRN", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_HRS_P2_HRN_RCN_EXP_DCN_EXP_TopHU_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: "REQ", topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "HRN", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");

		#endregion

		#region Tests_MixedDirection_TopHU_2_Inners

		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_IMP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_DOM_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_DOM_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_IMP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_Unknown_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_IMP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "", dcnDirection: "IMP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_DOM_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "", dcnDirection: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_Unknown_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "DOM", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_Unknown_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_OVR_RCN_IMP_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "OVR", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_OVR_RCN_EXP_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "OVR", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_OVR_RCN_IMP_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_OVR_RCN_EXP_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_NOT_RCN_EXP_DCN_IMP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "NOT", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "HRS", expectedP2Stat: "NOT", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_SEC_RCN_EXP_DCN_IMP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "HRS", expectedP2Stat: "SEC", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_HRN_RCN_IMP_DCN_IMP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRN_P2_HRN_RCN_EXP_DCN_EXP_TopHU_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRN", p2Stat: "HRN", midOuterStat: null, topOuterStat: "HRN", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "HRN", expectedP2Stat: "HRN", expectedTopOuterStat: "HRN", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);

		#endregion

		#region Tests_MixedDirection_TopOVP_2_Inners

		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "HRN", rcnDirection: "DOM", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "HRN", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_DOM_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "EXP", dcnDirection: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_DOM_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "DOM", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_IMP_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "", dcnDirection: "IMP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_DOM_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "OVR", rcnDirection: "", dcnDirection: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_DOM_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "DOM", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "EXP", dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_Unknown_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "SCR", rcnDirection: "", dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "SCR", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_OVR_RCN_IMP_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_OVR_RCN_EXP_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_OVR_RCN_IMP_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: null, topOuterStat: "NOT", rcnDirection: "IMP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_OVR_RCN_EXP_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_NOT_RCN_EXP_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "NOT", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "HRS", expectedP2Stat: "NOT", expectedTopOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_SEC_RCN_EXP_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: null, topOuterStat: "HRN", rcnDirection: "EXP", dcnDirection: "IMP", expectedP1Stat: "HRS", expectedP2Stat: "SEC", expectedTopOuterStat: "HRN", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_HRN_RCN_IMP_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_HRN_RCN_EXP_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "EXP", dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "HRN", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);

		#endregion

		#endregion

		#region Tests_NoSecurityProcessing

		public void Test_TopHU_MidHU()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "REQ", midOuterStat: "HRS", topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "HRS", expectedP2Stat: "REQ", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "HU", expectedMidOuterStat: "HRS", processingRequired: 0);
		public void Test_TopOVP_MidOVP()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "REQ", midOuterStat: "SCR", topOuterStat: "SEC", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "HRS", expectedP2Stat: "REQ", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "SCR", processingRequired: 0);
		public void Test_TopHU_MidOVP()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "REQ", midOuterStat: "SCR", topOuterStat: "HRS", rcnDirection: "IMP", dcnDirection: "DOM", expectedP1Stat: "HRS", expectedP2Stat: "REQ", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: "OVP", expectedMidOuterStat: "SCR", processingRequired: 0);

		#endregion

		#region Tests_EdgeCases_MultipleRCNAndDCNs_TopHU_MidHU_2_Inners

		public void Test_P1_REQ_P2_REQ_RCN1_EXP_RCN2_IMP_DCN1_Unknown_DCN2_DOM_MultiRCNDCN_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "REQ", rcn1Direction: "EXP", rcn2Direction: "IMP", dcn1Direction: "", dcn2Direction: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_REQ_RCN1_EXP_RCN2_IMP_DCN1_IMP_DCN2_DOM_MultiRCNDCN_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "REQ", rcn1Direction: "EXP", rcn2Direction: "IMP", dcn1Direction: "IMP", dcn2Direction: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_NOT_RCN1_EXP_RCN2_IMP_DCN1_IMP_DCN2_DOM_MultiRCNDCN_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "REQ", rcn1Direction: "EXP", rcn2Direction: "IMP", dcn1Direction: "IMP", dcn2Direction: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_NOT_P2_REQ_RCN1_EXP_RCN2_IMP_DCN1_IMP_DCN2_DOM_MultiRCNDCN_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "NOT", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "REQ", rcn1Direction: "EXP", rcn2Direction: "IMP", dcn1Direction: "IMP", dcn2Direction: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "NOT");
		public void Test_P1_HRS_P2_SEC_RCN1_IMP_RCN2_Unknown_DCN1_IMP_DCN2_EXP_MultiRCNDCN_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: "HRS", topOuterStat: "HRS", rcn1Direction: "IMP", rcn2Direction: "", dcn1Direction: "IMP", dcn2Direction: "EXP", expectedP1Stat: "NOT", expectedP2Stat: "SEC", expectedTopOuterStat: "NOT", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "NOT");
		public void Test_P1_REQ_P2_REQ_RCN1_EXP_RCN2_Unknown_DCN1_Unknown_DCN2_EXP_MultiRCNDCN_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "REQ", rcn1Direction: "EXP", rcn2Direction: "", dcn1Direction: "", dcn2Direction: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "REQ", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "REQ");
		public void Test_P1_HRS_P2_SEC_RCN1_EXP_RCN2_Unknown_DCN1_Unknown_DCN2_EXP_MultiRCNDCN_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: "HRS", topOuterStat: "HRS", rcn1Direction: "EXP", rcn2Direction: "", dcn1Direction: "", dcn2Direction: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "SEC", expectedTopOuterStat: "HRS", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "HRS");
		public void Test_P1_HRS_P2_SEC_RCN1_EXP_RCN2_Unknown_DCN1_IMP_DCN2_EXP_MultiRCNDCN_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: "HRS", topOuterStat: "HRS", rcn1Direction: "EXP", rcn2Direction: "", dcn1Direction: "IMP", dcn2Direction: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "SEC", expectedTopOuterStat: "HRS", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "HRS");
		public void Test_P1_HRS_P2_NOT_RCN1_IMP_RCN2_Unknown_DCN1_Unknown_DCN2_Unknown_MultiRCNDCN_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "HRS", p2Stat: "NOT", midOuterStat: "HRS", topOuterStat: "HRS", rcn1Direction: "IMP", rcn2Direction: "", dcn1Direction: "", dcn2Direction: "", expectedP1Stat: "HRS", expectedP2Stat: "NOT", expectedTopOuterStat: "HRS", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "HRS");
		public void Test_P1_HRS_P2_NOT_RCN1_IMP_RCN2_Unknown_DCN1_Unknown_DCN2_EXP_MultiRCNDCN_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "HRS", p2Stat: "NOT", midOuterStat: "HRS", topOuterStat: "HRS", rcn1Direction: "IMP", rcn2Direction: "", dcn1Direction: "", dcn2Direction: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "NOT", expectedTopOuterStat: "HRS", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "HRS");
		public void Test_P1_NOT_P2_NOT_RCN1_Unknown_RCN2_Unknown_DCN1_Unknown_DCN2_EXP_MultiRCNDCN_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: "NOT", topOuterStat: "NOT", rcn1Direction: "", rcn2Direction: "", dcn1Direction: "", dcn2Direction: "EXP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "NOT");
		public void Test_P1_HRS_P2_SEC_RCN1_Unknown_RCN2_Unknown_DCN1_Unknown_DCN2_Unknown_MultiRCNDCN_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: "HRS", topOuterStat: "HRS", rcn1Direction: "", rcn2Direction: "", dcn1Direction: "", dcn2Direction: "", expectedP1Stat: "HRS", expectedP2Stat: "SEC", expectedTopOuterStat: "HRS", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "HRS");
		public void Test_P1_NOT_P2_SEC_RCN1_EXP_RCN2_IMP_DCN1_Unknown_DCN2_Unknown_MultiRCNDCN_TopHU_MidHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "NOT", p2Stat: "SEC", midOuterStat: "NOT", topOuterStat: "NOT", rcn1Direction: "EXP", rcn2Direction: "IMP", dcn1Direction: "", dcn2Direction: "", expectedP1Stat: "NOT", expectedP2Stat: "SEC", expectedTopOuterStat: "NOT", midOuterUnitType: "HU", topOuterUnitType: "HU", expectedMidOuterStat: "NOT");

		#endregion

		#region Tests_EdgeCases_MultipleRCNAndDCNs_TopHU_2_Inners

		public void Test_P1_REQ_P2_REQ_RCN1_EXP_RCN2_IMP_DCN1_Unknown_DCN2_DOM_MultiRCNDCN_TopHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: null, topOuterStat: "REQ", rcn1Direction: "EXP", rcn2Direction: "IMP", dcn1Direction: "", dcn2Direction: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_REQ_RCN1_EXP_RCN2_IMP_DCN1_IMP_DCN2_DOM_MultiRCNDCN_TopHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: null, topOuterStat: "REQ", rcn1Direction: "EXP", rcn2Direction: "IMP", dcn1Direction: "IMP", dcn2Direction: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_NOT_RCN1_EXP_RCN2_IMP_DCN1_IMP_DCN2_DOM_MultiRCNDCN_TopHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: null, topOuterStat: "REQ", rcn1Direction: "EXP", rcn2Direction: "IMP", dcn1Direction: "IMP", dcn2Direction: "DOM", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_REQ_RCN1_EXP_RCN2_IMP_DCN1_IMP_DCN2_DOM_MultiRCNDCN_TopHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "NOT", p2Stat: "REQ", midOuterStat: null, topOuterStat: "REQ", rcn1Direction: "EXP", rcn2Direction: "IMP", dcn1Direction: "IMP", dcn2Direction: "DOM", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_SEC_RCN1_IMP_RCN2_Unknown_DCN1_IMP_DCN2_EXP_MultiRCNDCN_TopHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: null, topOuterStat: "HRS", rcn1Direction: "IMP", rcn2Direction: "", dcn1Direction: "IMP", dcn2Direction: "EXP", expectedP1Stat: "NOT", expectedP2Stat: "SEC", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_REQ_RCN1_EXP_RCN2_Unknown_DCN1_Unknown_DCN2_EXP_MultiRCNDCN_TopHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: null, topOuterStat: "REQ", rcn1Direction: "EXP", rcn2Direction: "", dcn1Direction: "", dcn2Direction: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "REQ", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_SEC_RCN1_EXP_RCN2_Unknown_DCN1_Unknown_DCN2_EXP_MultiRCNDCN_TopHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: null, topOuterStat: "HRS", rcn1Direction: "EXP", rcn2Direction: "", dcn1Direction: "", dcn2Direction: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "SEC", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_SEC_RCN1_EXP_RCN2_Unknown_DCN1_IMP_DCN2_EXP_MultiRCNDCN_TopHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: null, topOuterStat: "HRS", rcn1Direction: "EXP", rcn2Direction: "", dcn1Direction: "IMP", dcn2Direction: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "SEC", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_NOT_RCN1_IMP_RCN2_Unknown_DCN1_Unknown_DCN2_Unknown_MultiRCNDCN_TopHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "HRS", p2Stat: "NOT", midOuterStat: null, topOuterStat: "HRS", rcn1Direction: "IMP", rcn2Direction: "", dcn1Direction: "", dcn2Direction: "", expectedP1Stat: "HRS", expectedP2Stat: "NOT", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_NOT_RCN1_IMP_RCN2_Unknown_DCN1_Unknown_DCN2_EXP_MultiRCNDCN_TopHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "HRS", p2Stat: "NOT", midOuterStat: null, topOuterStat: "HRS", rcn1Direction: "IMP", rcn2Direction: "", dcn1Direction: "", dcn2Direction: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "NOT", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_NOT_RCN1_Unknown_RCN2_Unknown_DCN1_Unknown_DCN2_EXP_MultiRCNDCN_TopHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "NOT", p2Stat: "NOT", midOuterStat: null, topOuterStat: "NOT", rcn1Direction: "", rcn2Direction: "", dcn1Direction: "", dcn2Direction: "EXP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_SEC_RCN1_Unknown_RCN2_Unknown_DCN1_Unknown_DCN2_Unknown_MultiRCNDCN_TopHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: null, topOuterStat: "HRS", rcn1Direction: "", rcn2Direction: "", dcn1Direction: "", dcn2Direction: "", expectedP1Stat: "HRS", expectedP2Stat: "SEC", expectedTopOuterStat: "HRS", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_SEC_RCN1_EXP_RCN2_IMP_DCN1_Unknown_DCN2_Unknown_MultiRCNDCN_TopHU_2_Inners()
			=> RunPackagesSecurityStatusMultiJobTestCore(p1Stat: "NOT", p2Stat: "SEC", midOuterStat: null, topOuterStat: "NOT", rcn1Direction: "EXP", rcn2Direction: "IMP", dcn1Direction: "", dcn2Direction: "", expectedP1Stat: "NOT", expectedP2Stat: "SEC", expectedTopOuterStat: "NOT", topOuterUnitType: "HU", midOuterUnitType: null, expectedMidOuterStat: null);

		#endregion

		#region Tests_EdgeCases_OnlyHasRCNOrDCN_TopOVP_MidOVP_2_Inners

		public void Test_P1_REQ_P2_NOT_RCN_null_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "NOT", rcnDirection: null, dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_NOT_RCN_EXP_DCN_null_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "SCR", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "SCR", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_null_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "REQ", topOuterStat: "SEC", rcnDirection: null, dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_null_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: "SCR", topOuterStat: "HRN", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "HRN", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "SCR");
		public void Test_P1_NOT_P2_OVR_RCN_null_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: "SEC", topOuterStat: "REQ", rcnDirection: null, dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "SEC");
		public void Test_P1_NOT_P2_OVR_RCN_EXP_DCN_null_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: "NOT", topOuterStat: "NOT", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_SEC_P2_SCR_RCN_null_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: "SEC", topOuterStat: "REQ", rcnDirection: null, dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "SEC");
		public void Test_P1_SEC_P2_SCR_RCN_Unknown_DCN_null_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: "", dcnDirection: null, expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_REQ_RCN_null_DCN_Unknown_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "SEC", rcnDirection: null, dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_REQ_P2_REQ_RCN_Unknown_DCN_null_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: "REQ", topOuterStat: "SCR", rcnDirection: "", dcnDirection: null, expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "SCR", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_HRS_P2_OVR_RCN_null_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "NOT", topOuterStat: "HRN", rcnDirection: null, dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "HRN", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_HRS_P2_OVR_RCN_EXP_DCN_null_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: "OVR", topOuterStat: "OVR", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "OVR");
		public void Test_P1_HRS_P2_NOT_RCN_null_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "OVR", rcnDirection: null, dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "OVR", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_HRS_P2_NOT_RCN_EXP_DCN_null_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "NOT", midOuterStat: "REQ", topOuterStat: "SEC", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "HRS", expectedP2Stat: "NOT", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");
		public void Test_P1_HRS_P2_SEC_RCN_null_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: "REQ", topOuterStat: "REQ", rcnDirection: null, dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_HRS_P2_SEC_RCN_EXP_DCN_null_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: "NOT", topOuterStat: "REQ", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "HRS", expectedP2Stat: "SEC", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_HRS_P2_HRN_RCN_null_DCN_IMP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: "REQ", topOuterStat: "NOT", rcnDirection: null, dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_HRS_P2_HRN_RCN_IMP_DCN_null_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: "SCR", topOuterStat: "NOT", rcnDirection: "IMP", dcnDirection: null, expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "NOT");
		public void Test_P1_HRS_P2_HRN_RCN_null_DCN_EXP_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: "SEC", topOuterStat: "HRN", rcnDirection: null, dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "HRN", expectedTopOuterStat: "HRN", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "SEC");
		public void Test_P1_HRS_P2_HRN_RCN_EXP_DCN_null_TopOVP_MidOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: "REQ", topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "HRS", expectedP2Stat: "HRN", expectedTopOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: "OVP", expectedMidOuterStat: "REQ");

		#endregion

		#region Tests_EdgeCases_OnlyHasRCNOrDCN_TopOVP_2_Inners

		public void Test_P1_REQ_P2_NOT_RCN_null_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: null, topOuterStat: "HRS", rcnDirection: null, dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_NOT_RCN_EXP_DCN_null_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "NOT", midOuterStat: null, topOuterStat: "HRN", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "REQ", expectedP2Stat: "NOT", expectedTopOuterStat: "HRN", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_null_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: null, dcnDirection: "EXP", expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_OVR_RCN_EXP_DCN_null_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "OVR", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "REQ", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_OVR_RCN_null_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: null, topOuterStat: "HRN", rcnDirection: null, dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "HRN", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_NOT_P2_OVR_RCN_EXP_DCN_null_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "HRS", midOuterStat: null, topOuterStat: "SCR", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "SEC", expectedP2Stat: "HRS", expectedTopOuterStat: "SCR", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_SCR_RCN_null_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: null, topOuterStat: "NOT", rcnDirection: null, dcnDirection: "EXP", expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_SEC_P2_SCR_RCN_Unknown_DCN_null_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "SEC", p2Stat: "SCR", midOuterStat: null, topOuterStat: "REQ", rcnDirection: "", dcnDirection: null, expectedP1Stat: "SEC", expectedP2Stat: "SCR", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_REQ_RCN_null_DCN_Unknown_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: null, topOuterStat: "REQ", rcnDirection: null, dcnDirection: "", expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_REQ_P2_REQ_RCN_Unknown_DCN_null_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "REQ", p2Stat: "REQ", midOuterStat: null, topOuterStat: "OVR", rcnDirection: "", dcnDirection: null, expectedP1Stat: "REQ", expectedP2Stat: "REQ", expectedTopOuterStat: "OVR", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_OVR_RCN_null_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: null, topOuterStat: "OVR", rcnDirection: null, dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "OVR", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_OVR_RCN_EXP_DCN_null_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "OVR", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "HRS", expectedP2Stat: "OVR", expectedTopOuterStat: "SEC", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_NOT_RCN_null_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "NOT", midOuterStat: null, topOuterStat: "SCR", rcnDirection: null, dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_NOT_RCN_EXP_DCN_null_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "NOT", midOuterStat: null, topOuterStat: "OVR", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "HRS", expectedP2Stat: "NOT", expectedTopOuterStat: "OVR", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_SEC_RCN_null_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: null, topOuterStat: "HRN", rcnDirection: null, dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_SEC_RCN_EXP_DCN_null_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "SEC", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "HRS", expectedP2Stat: "SEC", expectedTopOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_HRN_RCN_null_DCN_IMP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: null, topOuterStat: "HRN", rcnDirection: null, dcnDirection: "IMP", expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_HRN_RCN_IMP_DCN_null_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: null, topOuterStat: "SEC", rcnDirection: "IMP", dcnDirection: null, expectedP1Stat: "NOT", expectedP2Stat: "NOT", expectedTopOuterStat: "NOT", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_HRN_RCN_null_DCN_EXP_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: null, topOuterStat: "REQ", rcnDirection: null, dcnDirection: "EXP", expectedP1Stat: "HRS", expectedP2Stat: "HRN", expectedTopOuterStat: "REQ", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);
		public void Test_P1_HRS_P2_HRN_RCN_EXP_DCN_null_TopOVP_2_Inners()
			=> RunPackagesSecurityStatusTestCore(p1Stat: "HRS", p2Stat: "HRN", midOuterStat: null, topOuterStat: "HRS", rcnDirection: "EXP", dcnDirection: null, expectedP1Stat: "HRS", expectedP2Stat: "HRN", expectedTopOuterStat: "HRS", topOuterUnitType: "OVP", midOuterUnitType: null, expectedMidOuterStat: null);

		#endregion

		#region Tests_Core

		void RunPackagesSecurityStatusTestCore(string p1Stat, string p2Stat, string midOuterStat, string topOuterStat, string rcnDirection, string dcnDirection, string expectedP1Stat, string expectedP2Stat, string expectedTopOuterStat, string topOuterUnitType, string expectedMidOuterStat, string midOuterUnitType, int processingRequired = 1)
		{
			var (pkg1Pk, pkg2Pk, topOuterPk, midOuterPk) = CreateWhsPkgData(p1Stat: p1Stat, p2Stat: p2Stat, rcn1Direction: rcnDirection, dcn1Direction: dcnDirection, rcn2Direction: null, dcn2Direction: null,
				topOuterUnitType: topOuterUnitType, midOuterUnitType: midOuterUnitType, topOuterSecurityStatus: topOuterStat, midOuterSecurityStatus: midOuterStat);

			UpdateWhsSecurityProcessingFlag(processingRequired, warehouse.PK);

			RunTransformation();
			AssertAllFromDB(pkg1Pk, pkg2Pk, topOuterPk, midOuterPk, expectedP1Stat, expectedP2Stat, expectedTopOuterStat, expectedMidOuterStat);

			RunTransformation();
			AssertAllFromDB(pkg1Pk, pkg2Pk, topOuterPk, midOuterPk, expectedP1Stat, expectedP2Stat, expectedTopOuterStat, expectedMidOuterStat);
		}

		void RunPackagesSecurityStatusMultiJobTestCore(string p1Stat, string p2Stat, string rcn1Direction, string rcn2Direction, string dcn1Direction, string dcn2Direction, string expectedP1Stat, string expectedP2Stat, string expectedTopOuterStat, string topOuterUnitType, string midOuterUnitType, string midOuterStat, string topOuterStat, string expectedMidOuterStat, int processingRequired = 1)
		{
			var (pkg1Pk, pkg2Pk, topOuterPk, midOuterPk) = CreateWhsPkgData(p1Stat: p1Stat, p2Stat: p2Stat, rcn1Direction: rcn1Direction, dcn1Direction: dcn1Direction, rcn2Direction: rcn2Direction, dcn2Direction: dcn2Direction,
				topOuterUnitType: topOuterUnitType, midOuterUnitType: midOuterUnitType, topOuterSecurityStatus: topOuterStat, midOuterSecurityStatus: midOuterStat);

			UpdateWhsSecurityProcessingFlag(processingRequired, warehouse.PK);

			RunTransformation();
			AssertAllFromDB(pkg1Pk, pkg2Pk, topOuterPk, midOuterPk, expectedP1Stat, expectedP2Stat, expectedTopOuterStat, expectedMidOuterStat);

			RunTransformation();
			AssertAllFromDB(pkg1Pk, pkg2Pk, topOuterPk, midOuterPk, expectedP1Stat, expectedP2Stat, expectedTopOuterStat, expectedMidOuterStat);
		}

		#endregion

		#region TestUtils

		void AssertAllFromDB(Guid pkg1Pk, Guid pkg2Pk, Guid topOuterPk, Guid? midOuterPk, string expectedp1Stat, string expectedp2Stat, string expectedTopOuterStat, string expectedMidOuterStat = null)
		{
			WhsItemPackageState.AssertFromDB(TestConnection, pkg1Pk)
				.ExpectEquals($"WPS_SecurityStatus should be {expectedp1Stat}", p => p.WPS_SecurityStatus, expectedp1Stat)
				.VerifyAll();
			WhsItemPackageState.AssertFromDB(TestConnection, pkg2Pk)
				.ExpectEquals($"WPS_SecurityStatus should be {expectedp2Stat}", p => p.WPS_SecurityStatus, expectedp2Stat)
				.VerifyAll();
			WhsItemPackageState.AssertFromDB(TestConnection, topOuterPk)
				.ExpectEquals($"Top HU WPS_SecurityStatus should be {expectedTopOuterStat}", p => p.WPS_SecurityStatus, expectedTopOuterStat)
				.VerifyAll();

			if (midOuterPk != null)
			{
				WhsItemPackageState.AssertFromDB(TestConnection, (Guid)midOuterPk)
				.ExpectEquals($"Mid HU WPS_SecurityStatus should be {expectedTopOuterStat}", p => p.WPS_SecurityStatus, expectedMidOuterStat ?? expectedTopOuterStat)
				.VerifyAll();
			}
		}

		void UpdateWhsSecurityProcessingFlag(int isSecurityProcessingRequired, Guid whsPK)
		{
			using (var cmd = Db.Connection.Command($@"
				UPDATE
					dbo.WhsWarehouse
				SET
					WW_TransitSecurityProcessingRequired = {isSecurityProcessingRequired},
					WW_SystemLastEditTimeUtc = GETDATE(),
					WW_SystemLastEditUser = 'GDS'
				WHERE
					WW_PK = '{whsPK}'
			"))
			{
				cmd.ExecuteNonQuery();
			}
		}

		(Guid pkg1Pk, Guid pkg2Pk, Guid topOuterPk, Guid? midOuterPk) CreateWhsPkgData(string p1Stat, string p2Stat, string rcn1Direction, string dcn1Direction, string rcn2Direction, string dcn2Direction, string topOuterUnitType, string midOuterUnitType, string topOuterSecurityStatus, string midOuterSecurityStatus)
		{
			var arePkgsUnderSameConsignments = rcn2Direction == null;
			var sql = new StringBuilder();

			var rcn1 = rcn1Direction == null ? null : new WhsItemReceiveConsignment(warehouse, "RCN1", "RCN1", "STD", "CNNJG") { WRC_Direction = rcn1Direction }.AppendInsertAndReturnObject(sql);
			var rcn2 = rcn2Direction == null ? null : new WhsItemReceiveConsignment(warehouse, "RCN2", "RCN2", "STD", "CNNJG") { WRC_Direction = rcn2Direction }.AppendInsertAndReturnObject(sql);
			var dcn1 = dcn1Direction == null ? null : new WhsItemDispatchConsignment(warehouse, "DCN1", "DCN1", "STD") { WDC_Direction = dcn1Direction }.AppendInsertAndReturnObject(sql);
			var dcn2 = dcn2Direction == null ? null : new WhsItemDispatchConsignment(warehouse, "DCN2", "DCN2", "STD") { WDC_Direction = dcn2Direction }.AppendInsertAndReturnObject(sql);

			var rcn3 = new WhsItemReceiveConsignment(warehouse, "RCN3", "RCN3", "STD", "CNNJG") { WRC_Direction = string.Empty }.AppendInsertAndReturnObject(sql);
			var dcn3 = new WhsItemDispatchConsignment(warehouse, "DCN3", "DCN3", "STD") { WDC_Direction = string.Empty }.AppendInsertAndReturnObject(sql);

			var rtu = new WhsItemReceiveTransportationUnit(warehouse, "RTU", stagingLocation1, "RTU").AppendInsertAndReturnObject(sql);

			var packageJobFromRTU = new PkgPackageJob(rtu.PK) { KJ_ParentTableCode = "WRH", KJ_JobID = "RCN" }.AppendInsertAndReturnObject(sql);
			var package1 = new PkgPackage(packageJobFromRTU, "PLT", 0).AppendInsertAndReturnObject(sql);
			var packageState1 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package1.PK, status: "ARV", warehouse, rcn: rcn1, rtu: rtu, dcn: dcn1, lastLocation: stagingLocation1, securityStatus: p1Stat);

			var package2 = new PkgPackage(packageJobFromRTU, "PLT", 0).AppendInsertAndReturnObject(sql);
			var packageState2 = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, package2.PK, status: "ARV", warehouse, rcn: arePkgsUnderSameConsignments ? rcn1 : rcn2, rtu: rtu, dcn: arePkgsUnderSameConsignments ? dcn1 : dcn2, lastLocation: stagingLocation1, securityStatus: p2Stat);

			var topOuterPkg = new PkgPackage(packageJobFromRTU, "PLT", 0).AppendInsertAndReturnObject(sql);
			var topOuterPkgState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, topOuterPkg.PK, status: "ARV", warehouse, rcn: arePkgsUnderSameConsignments ? rcn1 : rcn3, rtu: rtu, dcn: arePkgsUnderSameConsignments ? dcn1 : dcn3,
				lastLocation: stagingLocation1, isHandlingUnit: true, unitType: topOuterUnitType, securityStatus: topOuterSecurityStatus);

			var pksToUpdatetopOuter = $"'{package1.PK}', '{package2.PK}'";

			Guid? midOuterPkgStatePK = null;
			if (midOuterUnitType != null)
			{
				var midOuterPkg = new PkgPackage(packageJobFromRTU, "PLT", 0).AppendInsertAndReturnObject(sql);
				var midOuterPkgState = TestWhsDataSetupHelper.CreateWhsItemPackageState(sql, midOuterPkg.PK, status: "ARV", warehouse, rcn: arePkgsUnderSameConsignments ? rcn1 : rcn3, rtu: rtu, dcn: arePkgsUnderSameConsignments ? dcn1 : dcn3,
					lastLocation: stagingLocation1, isHandlingUnit: true, unitType: midOuterUnitType, securityStatus: midOuterSecurityStatus ?? topOuterSecurityStatus);

				pksToUpdatetopOuter += $", '{midOuterPkg.PK}'";

				midOuterPkgStatePK = midOuterPkgState.PK;

				var divot1 = new PkgPackageHandlingUnitDivot(topOuterPkg.PK, midOuterPkg.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "GDS" }.AppendInsertAndReturnObject(sql);
				var divot2 = new PkgPackageHandlingUnitDivot(midOuterPkg.PK, package1.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "GDS" }.AppendInsertAndReturnObject(sql);
				var divot3 = new PkgPackageHandlingUnitDivot(midOuterPkg.PK, package2.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "GDS" }.AppendInsertAndReturnObject(sql);
			}
			else
			{
				var divot1 = new PkgPackageHandlingUnitDivot(topOuterPkg.PK, package1.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "GDS" }.AppendInsertAndReturnObject(sql);
				var divot2 = new PkgPackageHandlingUnitDivot(topOuterPkg.PK, package2.PK) { KPD_PackedTime = new DateTimeOffset(2021, 6, 13, 0, 0, 0, TimeSpan.FromHours(0)), KPD_GS_NKPackedUser = "GDS" }.AppendInsertAndReturnObject(sql);
			}

			sql.AppendLine($"UPDATE dbo.PkgPackage SET KP_KP_TopHandlingUnitPackage = '{topOuterPkg.PK}', KP_SystemLastEditTimeUtc = GETDATE(), KP_SystemLastEditUser = 'GDS'" +
				$" WHERE KP_PK IN ({pksToUpdatetopOuter});");

			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_PkgPackage_EnsureChildPackagesHaveSameTopLevelHandlingUnit, PkgPackageSchema.Constants.TableName, TestConnection))
			using (TestWhsDataSetupHelper.SuspendTrigger(TestWhsDataSetupHelper.TG_PkgPackageHandlingUnitDivot_EnsureDivotLinkedPackagesHaveSameTopLevelHandlingUnit, PkgPackageHandlingUnitDivotSchema.Constants.TableName, TestConnection))
			{
				TestConnection.ExecuteNonQuery(sql.ToString());
			}

			return (packageState1.PK, packageState2.PK, topOuterPkgState.PK, midOuterPkgStatePK);
		}

		#endregion

		#region Setup and Implementation

		protected override void SetUp()
		{
			base.SetUp();

			var sql = new StringBuilder();

			branch = new GlbBranch("BR1") { GB_RL_NKHomePort = "CNNJG" }.InsertAndReturnObject(TestConnection);
			warehouse = new WhsWarehouse("WH1", "TRW", branch.PK).WithDockDoor(TestConnection);
			var row1 = new WhsRow(warehouse, "R1").AppendInsertAndReturnObject(sql);
			var area1 = new WhsArea(warehouse.PK, "A1").AppendInsertAndReturnObject(sql);
			stagingLocation1 = new WhsLocation(row1.PK, area1.PK, area1.PK) { WL_Column = 1, WL_Level = 1, WL_Tray = 1 }.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToString());
		}

		WhsWarehouse warehouse;
		GlbBranch branch;
		WhsLocation stagingLocation1;

		protected override DataTransformation GetNewTestTransformationInstance() => new UpdatePackageStateSecurityStatusConsideringTransitJobDirection();

		public override string[] expectedIndex =>
		[
			"NONCLUSTERED INDEX [_WTG__Update Package Security Status considering consignment direction_1] ON [dbo].[WhsItemReceiveConsignment] ([WRC_Direction]) INCLUDE ([WRC_PK]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update Package Security Status considering consignment direction_2] ON [dbo].[WhsItemDispatchConsignment] ([WDC_Direction]) INCLUDE ([WDC_PK]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update Package Security Status considering consignment direction_3] ON [dbo].[WhsItemPackageState] ([WPS_AdjustedOut], [WPS_UnitType], [WPS_SecurityStatus]) INCLUDE ([WPS_KP_Package], [WPS_Status], [WPS_WDC_TransitDispatchConsignment], [WPS_WDH_TransitDispatchHeader], [WPS_WRC_TransitReceiveConsignment], [WPS_WW_Warehouse]) WHERE (([WPS_UnitType] IN ('OVP', 'PKL', 'PKG')) AND [WPS_AdjustedOut]='') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Update Package Security Status considering consignment direction_4] ON [dbo].[WhsItemPackageState] ([WPS_IsHandlingUnit], [WPS_UnitType], [WPS_SecurityStatus]) INCLUDE ([WPS_KP_Package]) WHERE ([WPS_IsHandlingUnit]=(1)) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		];

		#endregion
	}
}
