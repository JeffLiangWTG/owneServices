using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Customs.AE.Business;

public class VOYLine : ManifestLine
{
	public VOYLine() : base(numberOfFields) { }

	#region Override

	protected override void SetPreambleData()
	{
		SetField(Schema.RecordIdentifier, RecordIdentifier);
	}

	public override string RecordIdentifier
	{
		get { return "VOY"; }
	}

	#endregion

	#region Properties

	public ZString Identifier
	{
		get { return this[Schema.RecordIdentifier.Name]; }
	}

	public ZString LineCode
	{
		get { return this[Schema.LineCode.Name]; }
		set { SetField(Schema.LineCode, value); }
	}

	public ZString VoyageAgentCode
	{
		get { return this[Schema.VoyageAgentCode.Name]; }
		set { SetField(Schema.VoyageAgentCode, value); }
	}

	public ZString VesselName
	{
		get { return this[Schema.VesselName.Name]; }
		set { SetField(Schema.VesselName, value); }
	}

	public ZString AgentVoyageNumber
	{
		get { return this[Schema.AgentVoyageNumber.Name]; }
		set { SetField(Schema.AgentVoyageNumber, value); }
	}

	public ZString PortCodeOfDischarge
	{
		get { return this[Schema.PortCodeOfDischarge.Name]; }
		set { SetField(Schema.PortCodeOfDischarge, value); }
	}

	public ZDateTime ExpectedToArriveDate
	{
		get { return GetFieldAsZDateTime(Schema.ExpectedToArriveDate); }
		set { SetField(Schema.ExpectedToArriveDate, value); }
	}

	public ZString RotationNumber
	{
		get { return this[Schema.RotationNumber.Name]; }
		set { SetField(Schema.RotationNumber, value); }
	}

	public ZString MessageType
	{
		get { return this[Schema.MessageType.Name]; }
		set { SetField(Schema.MessageType, value); }
	}

	public ZInt NoOfInstalment
	{
		get { return GetFieldAsZInt(Schema.NoOfInstalment.Name); }
		set { SetField(Schema.NoOfInstalment, value); }
	}

	public ZInt AgentsManifestSequenceNumber
	{
		get { return GetFieldAsZInt(Schema.AgentsManifestSequenceNumber.Name); }
		set { SetField(Schema.AgentsManifestSequenceNumber, value); }
	}

	#endregion

	public static class Schema
	{
		public static readonly FlatFileFieldProperty RecordIdentifier = new FlatFileFieldProperty(0, 3);
		public static readonly FlatFileFieldProperty LineCode = new FlatFileFieldProperty(1, 6);
		public static readonly FlatFileFieldProperty VoyageAgentCode = new FlatFileFieldProperty(2, 6);
		public static readonly FlatFileFieldProperty VesselName = new FlatFileFieldProperty(3, 30);
		public static readonly FlatFileFieldProperty AgentVoyageNumber = new FlatFileFieldProperty(4, 10);
		public static readonly FlatFileFieldProperty PortCodeOfDischarge = new FlatFileFieldProperty(5, 5);
		public static readonly FlatFileFieldProperty ExpectedToArriveDate = new FlatFileFieldProperty(6, 11);
		public static readonly FlatFileFieldProperty RotationNumber = new FlatFileFieldProperty(7, 6);
		public static readonly FlatFileFieldProperty MessageType = new FlatFileFieldProperty(8, 3);
		public static readonly FlatFileFieldProperty NoOfInstalment = new FlatFileFieldProperty(9, 3);
		public static readonly FlatFileFieldProperty AgentsManifestSequenceNumber = new FlatFileFieldProperty(10, 5);
	}

	const int numberOfFields = 11;
}
