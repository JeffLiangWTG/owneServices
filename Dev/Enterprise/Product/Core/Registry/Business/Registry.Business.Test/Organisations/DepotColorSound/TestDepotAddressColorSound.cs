using System.IO;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DepotAddressColorSound))]
	sealed class TestDepotAddressColorSound : RegistryBusinessObjectTemplateTestCase
	{
		public void TestAddressList()
		{
			var org = (IOrgHeader)Factory.New(ObjectFactory.GetType(typeof(Enterprise.MasterFiles.Integration.IOrgHeader)));
			var firstAddress = Factory.New<IOrgAddress>();
			var secondAddress = Factory.New<IOrgAddress>();

			org.OH_Code = "ORG";
			firstAddress.OA_Address1 = "FIRST ADDRESS";
			firstAddress.OA_OH = org.PK;
			secondAddress.OA_Address1 = "SECOND ADDRESS";
			secondAddress.OA_OH = org.PK;

			Factory.Save();

			DepotAddressColorSound depotAddressColorSound = (DepotAddressColorSound)GetNewBusinessObject();
			depotAddressColorSound.Organisation = org.PK;

			AssertEquals("Address list should have two addresses", 2, depotAddressColorSound.AddressList.Count);
		}

		public void TestSavingFileFromPath()
		{
			using (var file = TempFile.NewWithExtension(".mp3"))
			{
				var fileContent = File.ReadAllBytes(file.Filename);
				DepotAddressColorSound depotAddressColorSound = (DepotAddressColorSound)GetNewBusinessObject();

				depotAddressColorSound.MP3FileName = file.Filename;
				AssertNotNull("File name should be saved", depotAddressColorSound.MP3FileName);
				AssertEquals("File contents should be saved", fileContent, depotAddressColorSound.MP3FileContent);
			}
		}

		public void TestRemovingFile()
		{
			DepotAddressColorSound depotAddressColorSound = (DepotAddressColorSound)GetNewBusinessObject();

			using (var file = TempFile.NewWithExtension(".wav"))
			{
				var fileContent = File.ReadAllBytes(file.Filename);

				depotAddressColorSound.MP3FileName = file.Filename;
				AssertNotNull("PRECONDITION - File name should be saved", depotAddressColorSound.MP3FileName);
				AssertEquals("PRECONDITION - File contents should be saved", fileContent, depotAddressColorSound.MP3FileContent);
			}

			depotAddressColorSound.MP3FileName = ZString.Empty;
			AssertEquals("File name should be emtpy", ZString.Empty, depotAddressColorSound.MP3FileName);
			AssertEquals("File contents should be empty", ZBlob.Empty, depotAddressColorSound.MP3FileContent);
		}

		public void TestValidation()
		{
			var org = CreateOrg();
			var address = CreateAddress(org);
			Factory.Save();

			DepotAddressColorSound depotAddressColorSound = (DepotAddressColorSound)GetNewBusinessObject();
			depotAddressColorSound.Organisation = org.PK;
			depotAddressColorSound.Address = address.PK;
			depotAddressColorSound.Color = "000000000";
			depotAddressColorSound.MP3FileName = "MP3";
			depotAddressColorSound.MP3FileContent = ZBlob.FromUTF8("BLOOH");

			CombineAssertions("No errors should be displayed with valid values", delegate
			{
				AssertNoErrors("Organisation", depotAddressColorSound.OrganisationInfo);
				AssertNoErrors("Address", depotAddressColorSound.AddressInfo);
				AssertNoErrors("Color", depotAddressColorSound.ColorInfo);
				AssertNoErrors("MP3FileContent", depotAddressColorSound.MP3FileContentInfo);
				AssertNoErrors("MP3FileName", depotAddressColorSound.MP3FileNameInfo);
			});

			depotAddressColorSound = (DepotAddressColorSound)GetNewBusinessObject();
			depotAddressColorSound.Organisation = ZGuid.NewZGuid();
			depotAddressColorSound.Address = ZGuid.NewZGuid();
			depotAddressColorSound.Color = "NOTVALID1";
			depotAddressColorSound.MP3FileName = ZString.Empty;
			depotAddressColorSound.MP3FileContent = ZBlob.Empty;

			CombineAssertions("Errors should be displayed with invalid values", delegate
			{
				AssertHasErrors(depotAddressColorSound.OrganisationInfo);
				AssertHasErrors(depotAddressColorSound.AddressInfo);
				AssertHasErrors(depotAddressColorSound.ColorInfo);
			});

			CombineAssertions("No errors should be displayed if both Wave file and MP3 file are not entered", delegate
			{
				AssertNoErrors("MP3FileContent", depotAddressColorSound.MP3FileContentInfo);
				AssertNoErrors("MP3FileName", depotAddressColorSound.MP3FileNameInfo);
			});
		}

		public void TestSoundFileValidation()
		{
			var org = CreateOrg();
			var address = CreateAddress(org);
			Factory.Save();

			DepotAddressColorSound depotAddressColorSound = (DepotAddressColorSound)GetNewBusinessObject();
			depotAddressColorSound.Organisation = org.PK;
			depotAddressColorSound.Address = address.PK;
			depotAddressColorSound.Color = "000000000";

			depotAddressColorSound.MP3FileName = ZString.Empty;
			depotAddressColorSound.MP3FileContent = ZBlob.Empty;

			CombineAssertions("No file provided should not give an error", delegate
			{
				AssertNoErrors("MP3FileContent", depotAddressColorSound.MP3FileContentInfo);
				AssertNoErrors("MP3FileName", depotAddressColorSound.MP3FileNameInfo);
			});

			depotAddressColorSound.MP3FileName = "MP3";
			depotAddressColorSound.MP3FileContent = ZBlob.Empty;

			CombineAssertions("If sound file name is not empty, sound file content should be validated", delegate
			{
				AssertHasErrors(depotAddressColorSound.MP3FileContentInfo);
				AssertNoErrors("MP3FileName", depotAddressColorSound.MP3FileNameInfo);
			});

			depotAddressColorSound.MP3FileName = @"\\\.///";
			depotAddressColorSound.MP3FileContent = ZBlob.Empty;

			CombineAssertions("If sound file name is not accessible, both sound file content and sound file name should be invalid", delegate
			{
				AssertHasErrors(depotAddressColorSound.MP3FileContentInfo);
				AssertHasErrors(depotAddressColorSound.MP3FileNameInfo);
			});
		}

		public void TestCorrectSerialization()
		{
			var org = CreateOrg();
			var address = CreateAddress(org);
			Factory.Save();

			DepotAddressColorSoundForTest item = new DepotAddressColorSoundForTest(Factory);

			byte[] badUTF8 = new byte[] { 239, 191, 190 };

			item.Organisation = org.PK;
			item.Address = address.PK;
			item.Color = "000000000";
			item.MP3FileName = "MP3";
			item.MP3FileContent = badUTF8;

			AssertNoExceptionThrown("XML should parse on both import and export", () =>
			{
				MemoryStream stream = new MemoryStream();
				XmlWriter writer = XmlWriter.Create(stream);
				item.WriteElementsForTest(writer);
				writer.Flush();
				writer.Close();

				stream.Position = 0;
				DepotAddressColorSoundForTest newItem = new DepotAddressColorSoundForTest(Factory);
				item.ReadElementsForTest(new XmlReaderWrapper(XmlReader.Create(stream)));
			});

			AssertEquals("Binary values should be equal after XML serialization import/export", badUTF8, item.MP3FileContent);
		}

		IOrgHeader CreateOrg()
		{
			var org = Factory.New<IOrgHeader>();
			org.OH_Code = "ORG";
			org.OH_IsPackDepot = true;
			return org;
		}

		IOrgAddress CreateAddress(IOrgHeader org)
		{
			var address = Factory.New<IOrgAddress>();
			address.OA_Address1 = "ADDRESS";
			address.OA_Code = "CODE";

			address.OA_OH = org.PK;
			return address;
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DepotAddressColorSound(Factory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new DepotAddressColorSound(null, Factory);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
