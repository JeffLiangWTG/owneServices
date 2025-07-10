using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.AE.Business;

public class CTRLine : ManifestLine
{
	public CTRLine() : base(numberOfFields) { }

	#region Override

	protected override void SetPreambleData()
	{
		SetField(Schema.RecordIdentifier, RecordIdentifier);
	}

	public override string RecordIdentifier
	{
		get { return "CTR"; }
	}

	#endregion

	#region Properties

	public ZString Identifier
	{
		get { return this[Schema.RecordIdentifier.Name]; }
	}

	public ZString ContainerNumber
	{
		get { return this[Schema.ContainerNumber.Name]; }
		set { SetField(Schema.ContainerNumber, value); }
	}

	public ZString CheckDigit
	{
		get { return this[Schema.CheckDigit.Name]; }
		set { SetField(Schema.CheckDigit, value); }
	}

	public ZDecimal TareWeightInMT
	{
		get { return GetFieldAsZDecimal(Schema.TareWeightInMT.Name, Schema.TareWeightInMT.Length); }
		set { SetField(Schema.TareWeightInMT, value); }
	}

	public ZString SealNumber
	{
		get { return this[Schema.SealNumber.Name]; }
		set { SetField(Schema.SealNumber, value); }
	}

	#endregion

	public static class Schema
	{
		public static readonly FlatFileFieldProperty RecordIdentifier = new FlatFileFieldProperty(0, 3);
		public static readonly FlatFileFieldProperty ContainerNumber = new FlatFileFieldProperty(1, 10);
		public static readonly FlatFileFieldProperty CheckDigit = new FlatFileFieldProperty(2, 1);
		public static readonly FlatFileFieldProperty TareWeightInMT = new FlatFileFieldProperty(3, 1);
		public static readonly FlatFileFieldProperty SealNumber = new FlatFileFieldProperty(4, 10);
	}

	const int numberOfFields = 5;
}
