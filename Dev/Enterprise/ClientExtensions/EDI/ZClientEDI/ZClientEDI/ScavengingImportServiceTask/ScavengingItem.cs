using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;
using CargoWise.eHub.Common.Extensions;

namespace Enterprise.Client.EDI.ScavengingImportServiceTask
{
	public class ScavengingItem
	{
		public ScavengingItem(string contentEncodedAndCompressed, Guid pK)
		{
			using (var encodedStream = new MemoryStream(Encoding.UTF8.GetBytes(contentEncodedAndCompressed)))
			{
				try
			{
				using (var contentStream = encodedStream.DecodeAndDecompress())
				{
					Content = XElement.Load(contentStream);
				}
			}
			catch (InvalidDataException ex)
			{
				throw new InvalidDataException(string.Format(CultureInfo.InvariantCulture, "Data found corrupted when processing item: {0}", pK), ex);
			}
			}

			ID = pK;
		}

		public Guid ID { get; set; }
		public string Client { get; set; }
		public string Type { get; set; }
		public DateTime Date { get; set; }
		public XElement Content { get; private set; }
		public bool IsProcessed { get; set; }
		public bool IsImported { get; set; }
	}
}