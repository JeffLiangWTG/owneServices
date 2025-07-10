using CargoWise.Types;

namespace Enterprise.DataTransfer.Business
{
	public abstract class FlatFileFormat : IFlatFileFormat
	{
		public FlatFileFormat()
		{
			SetDefaults();
		}

		#region IFlatFileFormat Members

		/// <summary>
		/// Converts a FlatFileDataRow into a flat file text line
		/// </summary>
		public abstract ZString ConvertToLine(FlatFileDataRow row);

		/// <summary>
		/// Converts a flat file text line into a FlatFileDataRow
		/// </summary>
		public abstract FlatFileDataRow ConvertToRow(ZString rawRow);

		public abstract FileExtensionType FileExtensionForExport { get; }
		public abstract FileExtensionType FileExtensionForImport { get; }

		#endregion

		protected virtual string GetClientSpecificFileExtension()
		{
			return ZString.Empty;
		}

		protected virtual string GetClientSpecificFileExtensionDescription()
		{
			return ZString.Empty;
		}

		void SetDefaults()
		{
			FileExtensionFilterBuilder.ClientSpecificFileExtension.Value = GetClientSpecificFileExtension();
			FileExtensionFilterBuilder.ClientSpecificFileExtensionDescription.Value = GetClientSpecificFileExtensionDescription();
		}

		#region Expose Properties for TestCase
#if DEBUG

		public ZString ClientSpecificExtensionForTesting
		{
			get { return GetClientSpecificFileExtension(); }
		}

		public ZString ClientSpecificExtensionDescriptionForTesting
		{
			get { return GetClientSpecificFileExtensionDescription(); }
		}

#endif
		#endregion
	}
}
