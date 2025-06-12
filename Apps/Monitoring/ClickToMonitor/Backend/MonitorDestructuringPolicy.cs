using Serilog.Core;
using Serilog.Events;

namespace XH.XT.Monitoring.ClickToMonitor.Backend
{
  public class MonitorDestructuringPolicy : IDestructuringPolicy
  {
    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
    {
      try
      {
        if (value is MonitoringData data)
        {
          var properties = new List<LogEventProperty>()
          {
            new LogEventProperty("ctm_active" , new ScalarValue(data.IsActive)),
            new LogEventProperty("ctm_descr" , new ScalarValue(data.Description)),
            new LogEventProperty("ctm_notification_kind" , new ScalarValue(data.State)),
            new LogEventProperty("ctm_object" , new StructureValue(new[]
            {
              new LogEventProperty("name" , new ScalarValue(data.ObjectName)),
              new LogEventProperty("ref" , new ScalarValue(data.ObjectReference)),
              new LogEventProperty("type" , new ScalarValue(data.ObjectType)),
            })),
            new LogEventProperty("ctm_shortname" , new ScalarValue(data.ShortName)),
            new LogEventProperty("ctm_type" , new ScalarValue(data.Type)),
            new LogEventProperty("ctm_unique_id" , new ScalarValue(data.Id)),
            new LogEventProperty("time" , new ScalarValue(data.Timestamp)),
            new LogEventProperty("record" , new StructureValue(new[]
            {
              new LogEventProperty("db_key" , new ScalarValue(data.Message.Record.DatabseKey)),
              new LogEventProperty("mess_id" , new ScalarValue(data.Message.Record.MessageId)),
              new LogEventProperty("plain_id" , new ScalarValue(data.Message.Record.PlainId)),
              new LogEventProperty("start_time" , new ScalarValue(data.Message.Record.StartTime)),
            })),
            new LogEventProperty("message_ref" , new ScalarValue(data.Message.Reference)),
            new LogEventProperty("message_data" , new StructureValue(new[]
            {
              new LogEventProperty("acklevel" , new ScalarValue(data.Message.MessageData.AcknowledgementLevel)),
              new LogEventProperty("ackprot" , new ScalarValue(data.Message.MessageData.AcknowledgementPort)),
              new LogEventProperty("acktimeout" , new ScalarValue(data.Message.MessageData.AcknowledgementTimeout)),
              new LogEventProperty("archiveflags" , new ScalarValue(data.Message.MessageData.ArchiveFlags)),
              new LogEventProperty("archivepath" , new ScalarValue(data.Message.MessageData.ArchivePath)),
              new LogEventProperty("cfgversion" , new ScalarValue(data.Message.MessageData.ConfigurationVersion)),
              new LogEventProperty("contractobj" , new ScalarValue(data.Message.MessageData.ContractObject)),
              new LogEventProperty("creationtime" , new ScalarValue(data.Message.MessageData.CreationTime)),
              new LogEventProperty("curracklevel" , new ScalarValue(data.Message.MessageData.CurrentAcknowledgementLevel)),
              new LogEventProperty("datahashin" , new ScalarValue(data.Message.MessageData.DataHashIn)),
              new LogEventProperty("datahashout" , new ScalarValue(data.Message.MessageData.DataHashOut)),
              new LogEventProperty("datasizein" , new ScalarValue(data.Message.MessageData.DataSizeIn)),
              new LogEventProperty("datasizeout" , new ScalarValue(data.Message.MessageData.DataSizeOut)),
              new LogEventProperty("filenamein" , new ScalarValue(data.Message.MessageData.FileNameIn)),
              new LogEventProperty("filenameout" , new ScalarValue(data.Message.MessageData.FileNameOut)),
              new LogEventProperty("flags" , new ScalarValue(data.Message.MessageData.Flags)),
              new LogEventProperty("flagstext" , new ScalarValue(data.Message.MessageData.FlagsText)),
              new LogEventProperty("folderin" , new ScalarValue(data.Message.MessageData.FolderIn)),
              new LogEventProperty("folderout" , new ScalarValue(data.Message.MessageData.FolderOut)),
              new LogEventProperty("fromobj" , new ScalarValue(data.Message.MessageData.FromObject)),
              new LogEventProperty("fromparty" , new ScalarValue(data.Message.MessageData.FromParty)),
              new LogEventProperty("fromprot" , new ScalarValue(data.Message.MessageData.FromPort)),
              new LogEventProperty("internalid" , new ScalarValue(data.Message.MessageData.InternalId)),
              new LogEventProperty("laststate" , new ScalarValue(data.Message.MessageData.LastState)),
              new LogEventProperty("msgid" , new ScalarValue(data.Message.MessageData.MessageId)),
              new LogEventProperty("msginfo" , new ScalarValue(data.Message.MessageData.MessageInfo)),
              new LogEventProperty("msgtype" , new ScalarValue(data.Message.MessageData.MessageType)),
              new LogEventProperty("msguuid" , new ScalarValue(data.Message.MessageData.MessageUuid)),
              new LogEventProperty("owner" , new ScalarValue(data.Message.MessageData.Owner)),
              new LogEventProperty("priority" , new ScalarValue(data.Message.MessageData.Priority)),
              new LogEventProperty("remotecontract" , new ScalarValue(data.Message.MessageData.RemoteContract)),
              new LogEventProperty("seqidin" , new ScalarValue(data.Message.MessageData.SequenceIdIn)),
              new LogEventProperty("seqidout" , new ScalarValue(data.Message.MessageData.SequenceIdOut)),
              new LogEventProperty("seqnoin" , new ScalarValue(data.Message.MessageData.SequenceNumberIn)),
              new LogEventProperty("seqnoout" , new ScalarValue(data.Message.MessageData.SequenceNumberOut)),
              new LogEventProperty("seqtypein" , new ScalarValue(data.Message.MessageData.SequenceTypeIn)),
              new LogEventProperty("seqtypeout" , new ScalarValue(data.Message.MessageData.SequenceTypeOut)),
              new LogEventProperty("sequuidin" , new ScalarValue(data.Message.MessageData.SequenceUuidIn)),
              new LogEventProperty("sequuidout" , new ScalarValue(data.Message.MessageData.SequenceUuidOut)),
              new LogEventProperty("state" , new ScalarValue(data.Message.MessageData.State)),
              new LogEventProperty("statename" , new ScalarValue(data.Message.MessageData.StateName)),
              new LogEventProperty("syncreply" , new ScalarValue(data.Message.MessageData.SynchronousReply)),
              new LogEventProperty("toobj" , new ScalarValue(data.Message.MessageData.ToObject)),
              new LogEventProperty("toparty" , new ScalarValue(data.Message.MessageData.ToParty)),
              new LogEventProperty("toprot" , new ScalarValue(data.Message.MessageData.ToPort)),
              new LogEventProperty("version" , new ScalarValue(data.Message.MessageData.Version)),
              new LogEventProperty("wfinst" , new ScalarValue(data.Message.MessageData.WorkflowInstance)),
            })),
            new LogEventProperty("sample", new StructureValue(new[]
            {
              new LogEventProperty("alarming" , new ScalarValue(data.Queue.IsAlarming)),
              new LogEventProperty("db_key" , new ScalarValue(data.Queue.DatabaseKey)),
              new LogEventProperty("over_limit" , new ScalarValue(data.Queue.IsOverLimit)),
              new LogEventProperty("sample_qsize" , new ScalarValue(data.Queue.QueueSize)),
              new LogEventProperty("sample_time" , new ScalarValue(data.Queue.SampleTime)),
            })),
            new LogEventProperty("session_number_sample", new StructureValue(new[]
            {
              new LogEventProperty("alarming" , new ScalarValue(data.Session.IsAlarming)),
              new LogEventProperty("sample_number" , new ScalarValue(data.Session.SampleNumber)),
              new LogEventProperty("sample_time" , new ScalarValue(data.Session.SampleTime)),
              new LogEventProperty("db_key" , new ScalarValue(data.Session.DatabaseKey)),
            })),
            new LogEventProperty("transport_on_hold_samples", new StructureValue(new[]
            {
              new LogEventProperty("alarming" , new ScalarValue(data.Transport)),
              new LogEventProperty("result" , new StructureValue(new[]
              {
                new LogEventProperty("expire_time" , new ScalarValue(data.Transport)),
                new LogEventProperty("status" , new ScalarValue(data.Transport)),
                new LogEventProperty("transport" , new ScalarValue(data.Transport)),
              })),
              new LogEventProperty("sample_number" , new ScalarValue(data.Transport)),
              new LogEventProperty("sample_time" , new ScalarValue(data.Transport)),
              new LogEventProperty("db_key" , new ScalarValue(data.Transport)),
            }))
          };

          result = new StructureValue(properties);
          return true;
        }
        result = new ScalarValue(string.Empty);
        return true;
      }
      catch (Exception ex)
      {
        result = new ScalarValue(string.Empty);
        return true;
      }
    }
  }
}
