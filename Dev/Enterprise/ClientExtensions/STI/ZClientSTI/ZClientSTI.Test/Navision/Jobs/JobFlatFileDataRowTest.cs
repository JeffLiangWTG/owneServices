using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class JobFlatFileDataRowTest : TestCase
	{
		public void TestProperties()
		{
			JobFlatFileDataRow row = new JobFlatFileDataRow();
			AssertEquals("Row Should have 31 Fields", 30, row.FieldCount);
			row.JobNumber = "S000019864";
			row.SearchDescription = "searching for job";
			row.Description = "Order Reference 98274092374";
			row.Description2 = "Paladin";
			row.BillToCustomerNumber = "prestige";
			row.CreationDate = new ZDateTime(2006, 2, 2, 3, 2, 3);
			row.StartingDate = new ZDateTime(2006, 1, 1, 1, 1, 1);
			row.EndingDate = new ZDateTime(2006, 5, 5, 5, 5, 5);
			row.JobPostingDate = new ZDateTime(2006, 12, 12, 12, 12, 12);
			row.CompletionPercentage = 84;
			row.Status = "Order";
			row.PersonResponsible = "JLHFHFD";
			row.GlobalDimension1Code = "global";
			row.GlobalDimension2Code = "international";
			row.JobPostingGroup = "Group XXX";
			row.RecognitionMethod = "Recognizer";
			row.ApplicationMethod = "Manual";
			row.JobUsagePosting = "None";
			row.OwnerReference = "owner reference";
			row.ConsignorName = "Strang International";
			row.MasterAWB = "JKHJKH9879087";
			row.HouseAWB = "ASDFAF42343244";
			row.VesselFlight = "Q2934";
			row.TransportMode = "AIR";
			row.ContainerNumbers = "234 134 2345 78 879 970 3 8734";
			row.LoadingPort = "USLAX";
			row.DischargePort = "AUSYD";
			row.ConsolidatedETA = new ZDateTime(2006, 4, 4, 4, 4, 4);
			row.ConsolidatedETD = new ZDateTime(2006, 9, 9, 9, 9, 9);
			row.GoodsDescription = "Description of Goods - Weapons of Mass Destruction";
			AssertEquals("Job Number", "S000019864", row.JobNumber);
			AssertEquals("Search Description", "searching for job", row.SearchDescription);
			AssertEquals("Description", "Order Reference 98274092374", row.Description);
			AssertEquals("Description2", "Paladin", row.Description2);
			AssertEquals("Bill To Customer Number", "prestige", row.BillToCustomerNumber);
			AssertEquals("Creation Date", new ZDateTime(2006, 2, 2), row.CreationDate);
			AssertEquals("Starting Date", new ZDateTime(2006, 1, 1), row.StartingDate);
			AssertEquals("Ending Date", new ZDateTime(2006, 5, 5), row.EndingDate);
			AssertEquals("Job Posting Date", new ZDateTime(2006, 12, 12), row.JobPostingDate);
			AssertEquals("Completion Percentage", 84, row.CompletionPercentage);
			AssertEquals("Status", "Order", row.Status);
			AssertEquals("Person Responsible", "JLHFHFD", row.PersonResponsible);
			AssertEquals("Global Dimension 1 Code", "global", row.GlobalDimension1Code);
			AssertEquals("Global Dimension 2 Code", "international", row.GlobalDimension2Code);
			AssertEquals("Job Posting Group", "Group XXX", row.JobPostingGroup);
			AssertEquals("Recognition Method", "Recognizer", row.RecognitionMethod);
			AssertEquals("Application Method", "Manual", row.ApplicationMethod);
			AssertEquals("Job Usage Posting", "None", row.JobUsagePosting);
			AssertEquals("Owner Reference", "owner reference", row.OwnerReference);
			AssertEquals("Consignor Name", "Strang International", row.ConsignorName);
			AssertEquals("MasterAWB", "JKHJKH9879087", row.MasterAWB);
			AssertEquals("HouseAWB", "ASDFAF42343244", row.HouseAWB);
			AssertEquals("Vessel Flight", "Q2934", row.VesselFlight);
			AssertEquals("Transport Mode", "AIR", row.TransportMode);
			AssertEquals("Container Numbers", "234 134 2345 78 879 970 3 8734", row.ContainerNumbers);
			AssertEquals("Loading Port", "USLAX", row.LoadingPort);
			AssertEquals("Discharge Port", "AUSYD", row.DischargePort);
			AssertEquals("Consolidated ETA", new ZDateTime(2006, 4, 4), row.ConsolidatedETA);
			AssertEquals("Consolidated ETD", new ZDateTime(2006, 9, 9), row.ConsolidatedETD);
			AssertEquals("Goods Description", "Description of Goods - Weapons of Mass Destruction", row.GoodsDescription);
		}
	}
}
