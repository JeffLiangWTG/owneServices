using System;
using System.Drawing;
using System.IO;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base.Testing
{
	[TestedType(typeof(eDocWrapper))]
	sealed class eDocWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			CombineAssertions(delegate
			{
				var wrapper = new eDocWrapper(eDoc, Factory);
				var expectedImage = Image.FromStream(new MemoryStream((byte[])eDoc.ImageData));

				AssertEquals("Wrong DocType", eDoc.DocType, wrapper.DocumentType);
				AssertEquals("Wrong Description", eDoc.Description, wrapper.Description);
				AssertEquals("Wrong Date", eDoc.DateAdded, wrapper.DateAdded);
				AssertImageEquals("Wrong Image", expectedImage, wrapper.ImageData);
			});
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new eDocWrapper(eDoc, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
eDoc                                     (Default Field: DocumentType)
======================================================================
Name                                    Type
----------------------------------------------------------------------
DateAdded                               DateTime
Description                             String
DocumentType                            String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new eDocWrapper(eDoc, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentWrappers.Testing.Squares_100dpi.tif", "Squares_100dpi.tif");
			var shipment = Factory.New<ForwardingShipment>();
			eDoc = (shipment as IEDocsProvider).DocManagerInfo.AddFileOrDocument(Path.GetFullPath(tempFileName), "");
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;
		IeDoc eDoc;

		#endregion
	}
}
