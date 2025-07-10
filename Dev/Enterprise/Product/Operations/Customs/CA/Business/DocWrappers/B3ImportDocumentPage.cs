using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	internal partial class B3ImportDocumentPage : NonPersistentBusinessObject
	{
		public B3ImportDocumentPage(IB3SubHeader b3SubHeader)
		{
			B3SubHeader = b3SubHeader;
			lines = new ClassificationLine[5];
			linesCount = 0;
		}

		public IB3SubHeader B3SubHeader { get; }
		public IB3Header B3Header { get; private set; }
		public B3Footer B3Footer { get; private set; }

		public ZString ImporterFormatted
		{
			get
			{
				if (!_importerFormatted.HasValue)
				{
					_importerFormatted = ZString.Empty;
					if (B3Header != null)
					{
						var importer = B3Header.Importer;
						if (importer != null)
						{
							var address = importer as OrgAddress;
							if (address != null)
							{
								_importerFormatted = AdjustmentDocHelper.AddressForImporterFormatted(address);
							}
							else
							{
								var docAddress = importer as JobDocAddress;
								_importerFormatted = docAddress == null ? importer.E2_CompanyNameTruncated : AdjustmentDocHelper.AddressForImporterFormatted(docAddress);
							}
						}
					}
				}
				return _importerFormatted.Value;
			}
		}
		ZString? _importerFormatted;

		public ZString VendorFormatted
		{
			get
			{
				if (!_vendorFormatted.HasValue)
				{
					_vendorFormatted = ZString.Empty;
					if (B3SubHeader != null)
					{
						var vendor = B3SubHeader.Vendor;
						if (vendor != null)
						{
							var address = vendor as OrgAddress;
							if (address != null)
							{
								_vendorFormatted = AdjustmentDocHelper.AddressForVendorFormatted(address);
							}
							else
							{
								var docAddress = vendor as JobDocAddress;
								_vendorFormatted = docAddress != null ? AdjustmentDocHelper.AddressForVendorFormatted(docAddress) : (ZString)($@"{vendor.E2_CompanyNameTruncated}
{vendor.E2_State} {vendor.E2_Postcode}").TrimEnd();
							}
						}
					}
				}
				return _vendorFormatted.Value;
			}
		}
		ZString? _vendorFormatted;

		#region Lines

		public ClassificationLine Line1
		{
			get { return lines[0]; }
		}

		public ClassificationLine Line2
		{
			get { return lines[1]; }
		}

		public ClassificationLine Line3
		{
			get { return lines[2]; }
		}

		public ClassificationLine Line4
		{
			get { return lines[3]; }
		}

		public ClassificationLine Line5
		{
			get { return lines[4]; }
		}

		#endregion

		#region Implementation

		internal void AddHeader(IB3Header header)
		{
			B3Header = header;
		}

		internal void AddFooter(IB3Header header)
		{
			B3Footer = new B3Footer(header);
		}

		internal void AddLine(ClassificationLine line)
		{
			lines[linesCount++] = line;
		}

		internal bool CanAddLine
		{
			get { return linesCount < (B3Header == null || B3Header.B3Comments.IsEmpty ? 5 : 3); }
		}

		internal bool IsEmpty
		{
			get { return linesCount == 0; }
		}

		readonly ClassificationLine[] lines;
		int linesCount;

		#endregion
	}
}
