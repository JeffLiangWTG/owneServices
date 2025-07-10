using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.AE.Business;

public class ENDLine : ManifestLine
{
	public ENDLine() : base(numberOfFields) { }

	#region Override

	protected override void SetPreambleData()
	{
		SetField(Schema.RecordIdentifier, RecordIdentifier);
	}

	public override string RecordIdentifier
	{
		get { return "END"; }
	}

	#endregion

	#region Properties

	public ZString Identifier
	{
		get { return this[Schema.RecordIdentifier.Name]; }
	}

	public ZInt NoOfContainerRelatedBOL
	{
		get { return GetFieldAsZInt(Schema.NoOfContainerRelatedBOL.Name); }
		set { SetField(Schema.NoOfContainerRelatedBOL, value); }
	}

	public ZInt NoOfOtherBOL
	{
		get { return GetFieldAsZInt(Schema.NoOfOtherBOL.Name); }
		set { SetField(Schema.NoOfOtherBOL, value); }
	}

	public ZString Remarks
	{
		get { return this[Schema.Remarks.Name]; }
		set { SetField(Schema.Remarks, value); }
	}

	#endregion

	public static class Schema
	{
		public static readonly FlatFileFieldProperty RecordIdentifier = new FlatFileFieldProperty(0, 3);
		public static readonly FlatFileFieldProperty NoOfContainerRelatedBOL = new FlatFileFieldProperty(1, 4);
		public static readonly FlatFileFieldProperty NoOfOtherBOL = new FlatFileFieldProperty(2, 4);
		public static readonly FlatFileFieldProperty Remarks = new FlatFileFieldProperty(3, 100);
	}

	const int numberOfFields = 4;
}
