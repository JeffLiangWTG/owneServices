;WITH batch AS (
  SELECT TOP (@batchSize) *
    FROM eHubMessageEvent WITH (UPDLOCK, ROWLOCK, READPAST)
   WHERE ME_Status = 0
   ORDER BY ME_ID
)
UPDATE batch
   SET ME_Status = 1
OUTPUT inserted.ME_Message
