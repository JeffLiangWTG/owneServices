using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.CLE.OrdersDataImport.Testing
{
	public class CLEOrderFlatFileDataRowTest : TestCaseWithFactory
	{
		public void TestDeliveryPointUNLOCO()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_IsConsignee, SQLComparisonOperator.Equal, true);
			filter.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.Equal, "AUSYD");
			OrgHeader buyer = Factory.LoadTop1<OrgHeader>(filter);
			OrgAddress deliveryPointAddress = buyer.Addresses.AddNew();
			deliveryPointAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			deliveryPointAddress.OA_Code = "delivery point";
			filter = new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, buyer.PK);
			OrgHeader sourceOrg = Factory.LoadTop1<OrgHeader>(filter);
			OrgPatternMatchOverride sourceMatch = Factory.New<OrgPatternMatchOverride>();
			sourceMatch.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			sourceMatch.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			sourceMatch.OO_ForeignCode = "SYDAPP";
			sourceMatch.OO_LocalGuid = sourceOrg.PK;
			OrgPatternMatchOverride buyerMatch = Factory.New<OrgPatternMatchOverride>();
			buyerMatch.OO_OH = sourceOrg.PK;
			buyerMatch.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			buyerMatch.OO_ForeignCode = "UAIEVDWN";
			buyerMatch.OO_LocalGuid = buyer.PK;
			string csvOrderHeader = "SYDAPP,352326/46,20060102,AIR,LCL,20080912,USD,256.23,INC,AUSYDDBL,Australia Sydney Dibil,Address 1 line,Address 2 Line,Syndey,Don't know,25326,AU,Australia,AUSYD,UAIEVDWN,Ukrainian Kiev dowm,Buyer Address line 1,Buyer address line 2,Kiev,Kiev state,04213,UA,Ukraine,UAIEV,AUSYD,UAIEV,Some code...,Contact2,1,34.53.63,wisky,235.53,UQ,8.2,20080304,delivery point,special instractions,20080310,20080313,Attrib5,23.33,Y,customtext1";
			CLEOrderFlatFileDataRow orderRow = new CLEOrderFlatFileDataRow(new FlatFileDataRow(new OCsvLine(csvOrderHeader).FieldValues), Factory);
			AssertNotNull(orderRow);
			AssertEquals("SYDAPP", orderRow.Source);
			AssertEquals("UAIEVDWN", orderRow.BuyerCode);
			AssertEquals("delivery point", orderRow.DeliveryPoint);
			AssertEquals("AUMEL", orderRow.DeliveryPointUNLOCO);
			csvOrderHeader = "SYDAPP,352326/46,20060102,AIR,LCL,20080912,USD,256.23,INC,AUSYDDBL,Australia Sydney Dibil,Address 1 line,Address 2 Line,Syndey,Don't know,25326,AU,Australia,AUSYD,UAIEVDWN,Ukrainian Kiev dowm,Buyer Address line 1,Buyer address line 2,Kiev,Kiev state,04213,UA,Ukraine,UAIEV,AUSYD,UAIEV,Some code...,Contact2,1,34.53.63,wisky,235.53,UQ,8.2,20080304,xxxxxxxxxx,special instractions,20080310,20080313,Attrib5,23.33,Y,customtext1";
			orderRow = new CLEOrderFlatFileDataRow(new FlatFileDataRow(new OCsvLine(csvOrderHeader).FieldValues), Factory);
			AssertNotNull(orderRow);
			AssertEquals("SYDAPP", orderRow.Source);
			AssertEquals("UAIEVDWN", orderRow.BuyerCode);
			AssertEquals("xxxxxxxxxx", orderRow.DeliveryPoint);
			AssertEquals("AUSYD", orderRow.DeliveryPointUNLOCO);
		}

		public void TestProperties()
		{
			string csvOrderHeader = "SYDAPP,352326/46,20060102,AIR,LCL,20080912,USD,256.23,INC,AUSYDDBL,Australia Sydney Dibil,Address 1 line,Address 2 Line,Syndey,Don't know,25326,AU,Australia,AUSYD,UAIEVDWN,Ukrainian Kiev dowm,Buyer Address line 1,Buyer address line 2,Kiev,Kiev state,04213,UA,Ukraine,UAIEV,AUSYD,UAIEV,Some code...,Contact2,1,34.53.63,wisky,235.53,UQ,8.2,20080304,delivery point,special instractions,20080310,20080313,Attrib5,23.33,Y,customtext1";
			CLEOrderFlatFileDataRow orderRow = new CLEOrderFlatFileDataRow(new FlatFileDataRow(new OCsvLine(csvOrderHeader).FieldValues), Factory);
			AssertNotNull(orderRow);
			AssertEquals("SYDAPP", orderRow.Source);
			AssertEquals("352326/46", orderRow.OrderNumber);
			AssertEquals(new ZDateTime(2006, 1, 2), orderRow.RequiredExWorksDate);
			AssertEquals("AIR", orderRow.TransportMode);
			AssertEquals("LCL", orderRow.ContainerMode);
			AssertEquals(new ZDateTime(2008, 9, 12), orderRow.OrderDate);
			AssertEquals("USD", orderRow.Currency);
			AssertEquals(256.23m, orderRow.TotalOrderAmount);
			AssertEquals("INC", orderRow.INCOterm);
			AssertEquals("AUSYDDBL", orderRow.SupplierCode);
			AssertEquals("Australia Sydney Dibil", orderRow.SupplierCompanyName);
			AssertEquals("Address 1 line", orderRow.SupplierAddress1);
			AssertEquals("Address 2 Line", orderRow.SupplierAddress2);
			AssertEquals("Syndey", orderRow.SupplierCity);
			AssertEquals("Don't know", orderRow.SupplierState);
			AssertEquals("25326", orderRow.SupplierPostCode);
			AssertEquals("AU", orderRow.SupplierISOCountryCode);
			AssertEquals("Australia", orderRow.SupplierCountryName);
			AssertEquals("AUSYD", orderRow.SupplierUNLOCO);
			AssertEquals("UAIEVDWN", orderRow.BuyerCode);
			AssertEquals("Ukrainian Kiev dowm", orderRow.BuyerCompanyName);
			AssertEquals("Buyer Address line 1", orderRow.BuyerAddress1);
			AssertEquals("Buyer address line 2", orderRow.BuyerAddress2);
			AssertEquals("Kiev", orderRow.BuyerCity);
			AssertEquals("Kiev state", orderRow.BuyerState);
			AssertEquals("04213", orderRow.BuyerPostCode);
			AssertEquals("UA", orderRow.BuyerISOCountryCode);
			AssertEquals("Ukraine", orderRow.BuyerCountryName);
			AssertEquals("AUSYD", orderRow.LoadPort);
			AssertEquals("UAIEV", orderRow.DischargePort);
			AssertEquals("Some code...", orderRow.ControllingPartyCode);
			AssertEquals("Contact2", orderRow.Contact2);
			AssertEquals(1, orderRow.OrderLineNumber);
			AssertEquals("34.53.63", orderRow.ProductCode);
			AssertEquals("wisky", orderRow.ProductDescription);
			AssertEquals(235.53m, orderRow.Quantity);
			AssertEquals("UQ", orderRow.UQ);
			AssertEquals(8.2m, orderRow.TotalLinePrice);
			AssertEquals(new ZDateTime(2008, 3, 4), orderRow.RequiredIntoStoreDate);
			AssertEquals("delivery point", orderRow.DeliveryPoint);
			AssertEquals("special instractions", orderRow.SpecialInstractions);
			AssertEquals(new ZDateTime(2008, 3, 10), orderRow.CustomDate1);
			AssertEquals(new ZDateTime(2008, 3, 13), orderRow.CustomDate5);
			AssertEquals("Attrib5", orderRow.CustomAttrib5);
			AssertEquals(23.33m, orderRow.CustomDecimal5);
			AssertEquals("Y", orderRow.CustomFlag5);
			AssertEquals("customtext1", orderRow.CustomText1);
			AssertEquals("estimatedExFactoryDate", ZDateTime.Empty, orderRow.EstimatedExFactoryDate);
			csvOrderHeader = string.Concat(csvOrderHeader, ",20080912");
			orderRow = new CLEOrderFlatFileDataRow(new FlatFileDataRow(new OCsvLine(csvOrderHeader).FieldValues), Factory);
			AssertNotNull(orderRow);
			AssertEquals("estimatedExFactoryDate", new ZDateTime(2008, 9, 12), orderRow.EstimatedExFactoryDate);
			csvOrderHeader = "SYDAPP,352326/46,20060102,AIR,LCL,20080912,USD,256.23,INC,AUSYDDBL,Australia Sydney Dibil,Address 1 line,Address 2 Line,Syndey,Don't know,25326,AU,Australia,AUSYD,UAIEVDWN,Ukrainian Kiev dowm,Buyer Address line 1,Buyer address line 2,Kiev,Kiev state,04213,UA,Ukraine,UAIEV,AUSYD,UAIEV,Some code...,Contact2,1.00,34.53.63,wisky,235.53,UQ,8.2,20080304,delivery point,special instractions,20080310,20080313,Attrib5,23.33,Y,customtext1"; //Set OrderLineNumber to 1.00
			orderRow = new CLEOrderFlatFileDataRow(new FlatFileDataRow(new OCsvLine(csvOrderHeader).FieldValues), Factory);
			AssertNotNull(orderRow);
			AssertEquals(1, orderRow.OrderLineNumber);
		}
	}
}
