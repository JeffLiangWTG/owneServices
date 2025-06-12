using Newtonsoft.Json;

namespace XH.XT.Monitoring.ClickToMonitor.Backend
{
  public class ClickToMonitorData : BasicMonitoringData
  {
    [JsonProperty(PropertyName="overdue_messages")]
    public List<OverdueMessage> OverdueMessages { get; set; } = new();

    [JsonProperty(PropertyName = "samples")]
    public List<QueueSample> QueueSamples { get; set; } = new();

    [JsonProperty(PropertyName = "session_number_samples")]
    public List<SessionSample> SessionSamples { get; set; } = new();

    [JsonProperty(PropertyName = "transport_on_hold_samples")]
    public List<TransportOnHoldSample> TransportOnHoldSamples { get; set; } = new();
  }

  public class OverdueMessage
  {
    [JsonProperty(PropertyName = "message_ref")]
    public string Reference { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "record")]
    public Record Record { get; set; } = new();

    [JsonProperty(PropertyName = "message_data")]
    public MessageData MessageData { get; set; } = new();
  }

  public class Record
  {
    [JsonProperty(PropertyName = "db_key")]
    public string DatabseKey { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "mess_id")]
    public string MessageId { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "plain_id")]
    public string PlainId { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "start_time")]
    public long StartTime { get; set; }
  }

  public class MessageData
  {
    [JsonProperty(PropertyName = "acklevel")]
    public AcknowledgementLevel AcknowledgementLevel { get; set; }

    [JsonProperty(PropertyName = "ackprot")]
    public string AcknowledgementPort { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "acktimeout")]
    public string AcknowledgementTimeout { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "archiveflags")]
    public string ArchiveFlags { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "archivepath")]
    public string ArchivePath { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "cfgversion")]
    public string ConfigurationVersion { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "contractobj")]
    public string ContractObject { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "creationtime")]
    public string CreationTime { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "curracklevel")]
    public string CurrentAcknowledgementLevel { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "datahashin")]
    public string DataHashIn { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "datahashout")]
    public string DataHashOut { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "datasizein")]
    public string DataSizeIn { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "datasizeout")]
    public string DataSizeOut { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "filenamein")]
    public string FileNameIn { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "filenameout")]
    public string FileNameOut { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "flags")]
    public string Flags { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "flagstext")]
    public string FlagsText { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "folderin")]
    public string FolderIn { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "folderout")]
    public string FolderOut { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "fromobj")]
    public string FromObject { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "fromparty")]
    public string FromParty { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "fromprot")]
    public string FromPort { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "internalid")]
    public string InternalId { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "laststate")]
    public string LastState { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "msgid")]
    public string MessageId { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "msginfo")]
    public string MessageInfo { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "msgtype")]
    public string MessageType { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "msguuid")]
    public string MessageUuid { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "owner")]
    public string Owner { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "priority")]
    public string Priority { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "remotecontract")]
    public string RemoteContract { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "seqidin")]
    public string SequenceIdIn { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "seqidout")]
    public string SequenceIdOut { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "seqnoin")]
    public string SequenceNumberIn { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "seqnoout")]
    public string SequenceNumberOut { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "seqtypein")]
    public string SequenceTypeIn { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "seqtypeout")]
    public string SequenceTypeOut { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "sequuidin")]
    public string SequenceUuidIn { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "sequuidout")]
    public string SequenceUuidOut { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "state")]
    public string State { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "statename")]
    public string StateName { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "syncreply")]
    public string SynchronousReply { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "toobj")]
    public string ToObject { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "toparty")]
    public string ToParty { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "toprot")]
    public string ToPort { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "version")]
    public string Version { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "wfinst")]
    public string WorkflowInstance { get; set; } = string.Empty;
  }

  public class QueueSample
  {
    [JsonProperty(PropertyName = "alarming")]
    public bool IsAlarming { get; set; }

    [JsonProperty(PropertyName = "db_key")]
    public string DatabaseKey { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "over_limit")]
    public bool IsOverLimit { get; set; }

    [JsonProperty(PropertyName = "sample_qsize")]
    public int QueueSize { get; set; }

    [JsonProperty(PropertyName = "sample_time")]
    public long SampleTime { get; set; }
  }

  public class SessionSample
  {
    [JsonProperty(PropertyName = "alarming")]
    public bool IsAlarming { get; set; }

    [JsonProperty(PropertyName = "db_key")]
    public string DatabaseKey { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "sample_number")]
    public int SampleNumber { get; set; }

    [JsonProperty(PropertyName = "sample_time")]
    public long SampleTime { get; set; }
  }

  public class TransportOnHoldSample
  {
    [JsonProperty(PropertyName = "alarming")]
    public bool IsAlarming { get; set; }

    [JsonProperty(PropertyName = "sample_number")]
    public int SampleNumber { get; set; }

    [JsonProperty(PropertyName = "sample_time")]
    public long SampleTime { get; set; }

    [JsonProperty(PropertyName = "db_key")]
    public string DatabaseKey { get; set; } = string.Empty;
  }

  public class Result
  {
    [JsonProperty(PropertyName = "expire_time")]
    public long ExpireTime { get; set; }

    [JsonProperty(PropertyName = "status")]
    public TransportStatus Status { get; set; }

    [JsonProperty(PropertyName = "transport")]
    public Transport Transport { get; set; }
  }

  public enum AcknowledgementLevel
  {
    Delivery = 0,
    Application = 1
  }

  public enum TransportStatus
  {
    Hold = 0,
    Unhold = 1,
    Unhold_ok = 2
  }

  public enum Transport
  {
    xTRM = 1281,
    HttpClient = 1282,
    HttpServer = 1283,
    Smtp = 1284,
    FtpClient = 1285,
    FtpServer = 1286,
    Oftp = 1287,
    SftpClient = 1289,
    SftpServer = 1290,
    Mq = 1293,
    ApplicationTransport = 1294
  }
}
