using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class AWBDocumentTitleTest : TestCase
	{
		public void TestLoadingWhenNotAllReccordsEntered()
		{
			var data = System.Text.Encoding.UTF8.GetBytes(IncompleteHAWBDocumentTitlesData);

			var reg = new AWBDocumentTitle(data);

			AssertEquals("Original 1 - (for Issuing Carrier)", reg.Original1.Name);
			AssertEquals(true, reg.Original1.Printed);

			AssertEquals("Original 2 - (for Consignee)", reg.Original2.Name);
			AssertEquals(true, reg.Original2.Printed);

			AssertEquals(false, reg.Original3.Printed);

			AssertEquals("Copy 4 - (Delivery Receipt)", reg.Copy4.Name);
			AssertEquals(true, reg.Copy4.Printed);

			AssertEquals(false, reg.Copy5.Printed);
			AssertEquals(false, reg.Copy6.Printed);
			AssertEquals(false, reg.Copy7.Printed);
			AssertEquals(false, reg.Copy8.Printed);
		}

		public void TestLoadingWhenAllReccordsEntered()
		{
			var data = System.Text.Encoding.UTF8.GetBytes(CompleteHAWBDocumentTitlesData);

			var reg = new AWBDocumentTitle(data);

			AssertEquals("Original 1 - (for Issuing Carrier)", reg.Original1.Name);
			AssertEquals(true, reg.Original1.Printed);

			AssertEquals("Original 2 - (for Consignee)", reg.Original2.Name);
			AssertEquals(true, reg.Original2.Printed);

			AssertEquals("Original 3 - (for Shipper)", reg.Original3.Name);
			AssertEquals(true, reg.Original3.Printed);

			AssertEquals("Copy 4 - (Delivery Receipt)", reg.Copy4.Name);
			AssertEquals(true, reg.Copy4.Printed);

			AssertEquals("Copy 5 - (Extra Copy)", reg.Copy5.Name);
			AssertEquals(true, reg.Copy5.Printed);

			AssertEquals("Copy 6 - (Extra Copy)", reg.Copy6.Name);
			AssertEquals(true, reg.Copy6.Printed);

			AssertEquals("Copy 7 - (Extra Copy)", reg.Copy7.Name);
			AssertEquals(true, reg.Copy7.Printed);

			AssertEquals("Copy 8 - (for Agent)", reg.Copy8.Name);
			AssertEquals(true, reg.Copy8.Printed);
		}

		#region CompleteHAWBDocumentTitlesData

		const string CompleteHAWBDocumentTitlesData =
		@"<NewDataSet>
		    <ListTable>
			    <Name>Original 1 - (for Issuing Carrier)</Name>
			    <Title>Original 1 - (for Issuing Carrier)</Title>
			    <Printed>true</Printed>
		    </ListTable>
		    <ListTable>
			    <Name>Original 2 - (for Consignee)</Name>
			    <Title>Original 2 - (for Consignee)</Title>
			    <Printed>true</Printed>
		    </ListTable>
		    <ListTable>
			    <Name>Original 3 - (for Shipper)</Name>
			    <Title>Original 3 - (for Shipper)</Title>
			    <Printed>true</Printed>
		    </ListTable>
		    <ListTable>
			    <Name>Copy 4 - (Delivery Receipt)</Name>
			    <Title>Copy 4 - (Delivery Receipt)</Title>
			    <Printed>true</Printed>
		    </ListTable>
		    <ListTable>
			    <Name>Copy 5 - (Extra Copy)</Name>
			    <Title>Copy 5 - (Extra Copy)</Title>
			    <Printed>true</Printed>
		    </ListTable>
		    <ListTable>
			    <Name>Copy 6 - (Extra Copy)</Name>
			    <Title>Copy 6 - (Extra Copy)</Title>
			    <Printed>true</Printed>
		    </ListTable>
		    <ListTable>
			    <Name>Copy 7 - (Extra Copy)</Name>
			    <Title>Copy 7 - (Extra Copy)</Title>
			    <Printed>true</Printed>
		    </ListTable>
		    <ListTable>
			    <Name>Copy 8 - (for Agent)</Name>
			    <Title>Copy 8 - (for Agent)</Title>
			    <Printed>true</Printed>
		    </ListTable>
		</NewDataSet>";

		#endregion

		#region IncompleteHAWBDocumentTitlesData

		const string IncompleteHAWBDocumentTitlesData =
		@"<NewDataSet>
		    <ListTable>
			    <Name>Original 1 - (for Issuing Carrier)</Name>
			    <Title>Original 1 - (for Issuing Carrier)</Title>
			    <Printed>true</Printed>
		    </ListTable>
		    <ListTable>
			    <Name>Original 2 - (for Consignee)</Name>
			    <Title>Original 2 - (for Consignee)</Title>
			    <Printed>true</Printed>
		    </ListTable>
		    <ListTable>
			    <Name>Copy 4 - (Delivery Receipt)</Name>
			    <Title>Copy 4 - (Delivery Receipt)</Title>
			    <Printed>true</Printed>
		    </ListTable>
		</NewDataSet>";

		#endregion
	}
}
