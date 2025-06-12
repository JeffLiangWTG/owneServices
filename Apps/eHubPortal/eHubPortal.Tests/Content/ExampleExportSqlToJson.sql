SELECT '{' + STRING_AGG(TRIM('{}' FROM J),',') + '}' FROM (VALUES 
((SELECT * FROM [eHubOwner] FOR JSON AUTO, ROOT('eHubOwner'))),
((SELECT * FROM [eHubClient] FOR JSON AUTO, ROOT('eHubClient'))),
((SELECT * FROM [eHubTransformationSet] FOR JSON AUTO, ROOT('eHubTransformationSet'))),
((SELECT * FROM [eHubCodeSet] FOR JSON AUTO, ROOT('eHubCodeSet'))),
((SELECT * FROM [eHubCodeSetResult] FOR JSON AUTO, ROOT('eHubCodeSetResult'))),
((SELECT * FROM [eHubCodeMapKey] FOR JSON AUTO, ROOT('eHubCodeMapKey'))),
((SELECT * FROM [eHubCodeMapValue] FOR JSON AUTO, ROOT('eHubCodeMapValue')))
) T([J])
